using FAST.FBasicInteractiveConsole.TestCode;
using FAST.FBasicInterpreter;
using FAST.FBasicInterpreter.TemplatingLibrary;
using Microsoft.Data.Sqlite;

namespace FAST.FBasic.InteractiveConsole
{
    internal partial class FBasicIC
    {
        private void setupEnvironment()
        {
            this.env = new();
            env.DefaultEnvironment(programsFolder);
            env.callHandler = (name) => { var filepath = Path.Combine(programsFolder, name); return File.ReadAllText(filepath); };
            env.requestForObjectHandler = (context, group, name) =>
            {
                

                if ($"{context}.{group}.{name}" == "SQL.CONNECTION.MyCursor1")
                {
                    string cs = "<replace with your CS hear>";
                    var connection = new SqliteConnection(cs);
                    return connection;
                }

                if ($"{context}.{group}" == "SQL.CONNECTION")
                {
                    var demoDBcs = $"Data Source={getProgramsFolder()}\\Other\\demo.db";
                    if (!File.Exists($"{getProgramsFolder()}\\Other\\demo.db"))
                    {
                        new DemoDatabase().InstallDB(demoDBcs);
                    }
                    var connection = new SqliteConnection(demoDBcs);
                    return connection;
                }

                if ($"{context}.{group}.{name}" == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                if ($"{context}.{group}.{name}" == "IN.DB.DEMO") 
                {
                    var demoDBcs = $"Data Source={getProgramsFolder()}\\Other\\demo.db";
                    new DemoDatabase().QueryData(demoDBcs);
                    return true;
                }
                return null;
            };
            env.AddLibrary(new FBasicStringFunctions());
            env.AddLibrary(new FBasicMathFunctions());
            env.AddLibrary(new FBasicDateFunctions());
            env.AddLibrary(new FBasicSQLDataAdapter());
            env.AddLibrary(new FBasicEvents());
            env.AddLibrary(new FBasicTextReplacer());
            env.AddLibrary(new FBasicStack());
            env.AddLibrary(new FBasicDecisionTables());
            env.AddLibrary(new FBasic2DArrays());
            env.AddLibrary(new FBasicStreams());
            env.AddLibrary(new FBasicTemplatingLibrary());
            env.AddVariable("table.column", "myColumn1");

            FBasicEvents.Reset();
            FBasicEvents.FBasicEventHandler += testEventHandler1!;
            FBasicEvents.FBasicEventHandler += testEventHandler2!;
        }

        public void testEventHandler1(object sender, (string name, Stack<Value> args) e)
        {
            Console.WriteLine($"1st Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
        }
        public void testEventHandler2(object sender, (string name, Stack<Value> args) e)
        {
            Console.WriteLine($"2nd Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
        }
    }
}
