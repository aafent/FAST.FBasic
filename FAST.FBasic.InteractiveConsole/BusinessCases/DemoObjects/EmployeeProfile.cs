using System.Text.Json;

namespace FAST.FBasic.InteractiveConsole.DemoObjects
{
    // ----------------------------------------------------------------------
    // 1. Root Class (Mapping to the main JSON object)
    // Properties are now camelCase to match the JSON keys exactly,
    // and JsonPropertyName attributes have been removed.
    // ----------------------------------------------------------------------
    public record EmployeeProfile
    {
        // Basic types
        public string name { get; init; } = string.Empty;
        public int age { get; init; }
        public decimal salary { get; init; }
        public bool isActive { get; init; }
        public string email { get; init; } = string.Empty;

        // Nested Object
        public Address address { get; init; } = new Address();

        // Array of strings
        public List<string> hobbies { get; init; } = new List<string>();

        // Array of integers
        public List<int> scores { get; init; } = new List<int>();

        // Array of nested objects
        public List<Project> projects { get; init; } = new List<Project>();
    }

    // ----------------------------------------------------------------------
    // 2. Nested Class for the 'address' object
    // Properties are now camelCase.
    // ----------------------------------------------------------------------
    public record Address
    {
        public string street { get; init; } = string.Empty;
        public string city { get; init; } = string.Empty;
        public string zipCode { get; init; } = string.Empty;
    }

    // ----------------------------------------------------------------------
    // 3. Nested Class for items within the 'projects' array
    // Properties are now camelCase.
    // ----------------------------------------------------------------------
    public record Project
    {
        public string name { get; init; } = string.Empty;
        public string status { get; init; } = string.Empty;
    }

    // ----------------------------------------------------------------------
    // Example usage (Demonstrates how to actually deserialize the data)
    // Note: The example usage now accesses properties using their new camelCase names.
    // ----------------------------------------------------------------------
    public class EmployeeProfileExample
    {
        public EmployeeProfile GetEmployeeProfile()
        {
            // NOTE: JsonPropertyName is no longer needed because the C# properties
            // are named exactly like the JSON keys (camelCase).
            string json = @"{
            ""name"": ""John Doe"",
            ""age"": 30,
            ""salary"": 75000.50,
            ""isActive"": true,
            ""email"": ""john@example.com"",
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""New York"",
                ""zipCode"": ""10001""
            },
            ""hobbies"": [""reading"", ""gaming"", ""coding""],
            ""scores"": [95, 87, 92, 88],
            ""projects"": [
                {
                    ""name"": ""Project A"",
                    ""status"": ""completed""
                },
                {
                    ""name"": ""Project B"",
                    ""status"": ""in-progress""
                }
            ]
        }";

            // Deserialize the JSON string into the C# object
            var profile = JsonSerializer.Deserialize<EmployeeProfile>(json);

            return profile;
        }


        public void TestPrint(EmployeeProfile profile)
        {

            if (profile != null)
            {
                Console.WriteLine($"Name: {profile.name}, Age: {profile.age}");
                Console.WriteLine($"City: {profile.address.city}");
                Console.WriteLine($"First Hobby: {profile.hobbies[0]}");
                Console.WriteLine($"Project Count: {profile.projects.Count}");
            }
        }
    }

}