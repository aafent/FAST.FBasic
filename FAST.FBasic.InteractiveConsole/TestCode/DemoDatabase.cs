using Microsoft.Data.Sqlite;

namespace FAST.FBasicInteractiveConsole.TestCode
{
    // NuGet: Install-Package Microsoft.Data.Sqlite

    internal class DemoDatabase
    {
        public void InstallDB(string connectionString)
        {
            CreateDatabase(connectionString); // Create database and tables
            InsertSampleData(connectionString); // Insert sample data

            // Query data using ADO.NET
            //QueryData(connectionString);
        }

        private void CreateDatabase(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            // Create Customers table
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Customers (
                CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                VatNumber TEXT,
                Email TEXT,
                Address TEXT,
                City TEXT
            )";
            command.ExecuteNonQuery();

            // Create Orders table
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Orders (
                OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerID INTEGER,
                OrderDate TEXT,
                Amount REAL,
                FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
            )";
            command.ExecuteNonQuery();
        }

        private void InsertSampleData(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert customers
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                INSERT INTO Customers (Name, VatNumber, Email, Address, City) 
                VALUES (@name, @vat, @email, @address, @city)";

                string[][] customers = {
                new[] {"John Doe", "C54373226", "john@example.com",        "14,Times Str",        "New York"},
                new[] {"Jane Smith", "A65421345", "jane@example.com",      "88, Kings Avenue",    "London"},
                new[] {"Bob Johnson", "G65348876", "bob@example.com",      "4 Napoleon Str",      "Paris"},
                new[] {"Alice Williams", "A3560031", "alice@example.com",  "99 Chop Stick Avenue","Tokyo"},
                new[] {"Charlie Brown", "P45400032", "charlie@example.com","999 Kangaroo Sq.",    "Sydney"}
                };

                foreach (var customer in customers)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@name", customer[0]);
                    cmd.Parameters.AddWithValue("@vat", customer[1]);
                    cmd.Parameters.AddWithValue("@email", customer[2]);
                    cmd.Parameters.AddWithValue("@address", customer[3]);
                    cmd.Parameters.AddWithValue("@city", customer[4]);
                    cmd.ExecuteNonQuery();
                }

                // Insert orders
                cmd.CommandText = @"
                INSERT INTO Orders (CustomerID, OrderDate, Amount) 
                VALUES (@customerId, @orderDate, @amount)";

                var orders = new[] {
                new { CustomerId = 1, Date = "2024-01-15", Amount = 150.50 },
                new { CustomerId = 1, Date = "2024-02-20", Amount = 200.00 },
                new { CustomerId = 2, Date = "2024-01-18", Amount = 75.25 },
                new { CustomerId = 3, Date = "2024-03-10", Amount = 300.00 },
                new { CustomerId = 4, Date = "2024-02-25", Amount = 125.75 },
                new { CustomerId = 5, Date = "2024-03-15", Amount = 450.00 },
                new { CustomerId = 2, Date = "2024-03-20", Amount = 99.99 },
                new { CustomerId = 3, Date = "2024-01-30", Amount = 175.50 },
                new { CustomerId = 4, Date = "2024-03-05", Amount = 220.00 },
                new { CustomerId = 1, Date = "2024-03-22", Amount = 350.25 }
            };

                foreach (var order in orders)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@customerId", order.CustomerId);
                    cmd.Parameters.AddWithValue("@orderDate", order.Date);
                    cmd.Parameters.AddWithValue("@amount", order.Amount);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                Console.WriteLine("Sample data inserted successfully!");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error inserting data: {ex.Message}");
            }
        }

        public void QueryData(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Query with JOIN - standard ADO.NET
            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT c.Name, c.City, o.OrderDate, o.Amount
            FROM Customers c
            INNER JOIN Orders o ON c.CustomerID = o.CustomerID
            ORDER BY c.Name, o.OrderDate";

            using var reader = command.ExecuteReader();

            Console.WriteLine("\n=== Customer Orders ===");
            Console.WriteLine($"{"Name",-20} {"City",-15} {"Order Date",-12} {"Amount",10}");
            Console.WriteLine(new string('-', 60));

            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"],-20} {reader["City"],-15} {reader["OrderDate"],-12} {reader["Amount"],10:C}");
            }

            reader.Close();

            // Example: Parameterized query
            Console.WriteLine("\n=== Orders over $200 ===");
            command.CommandText = "SELECT * FROM Orders WHERE Amount > @minAmount";
            command.Parameters.AddWithValue("@minAmount", 200);

            using var reader2 = command.ExecuteReader();
            while (reader2.Read())
            {
                Console.WriteLine($"Order #{reader2["OrderID"]}: ${reader2["Amount"]} on {reader2["OrderDate"]}");
            }

            reader2.Close();
        }

    }
}
