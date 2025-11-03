using FAST.FBasic.InteractiveConsole.DemoObjects;
using FAST.FBasic.TemplatingLibrary;
using FAST.FBasicInteractiveConsole.TestCode;
using FAST.FBasicInterpreter;
using FAST.FBasicInterpreter.Types;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Dynamic;
using System.Text.Json;
using System.Xml.XPath;

namespace FAST.FBasic.InteractiveConsole
{
    internal partial class FBasicIC
    {
        private void setupEnvironment()
        {
            this.env = new();
            env.DefaultEnvironment(programsFolder);
            env.requestForObjectHandler = (request) =>
            {

                if ( request.Level3Request() == "SQL.CONNECTION.MyCursor1")
                {
                    string cs = "<replace with your CS hear>";
                    var connection = new SqliteConnection(cs);
                    return connection;
                }

                if ( request.Level2Request() == "SQL.CONNECTION")
                {
                    var demoDBcs = $"Data Source={getProgramsFolder()}\\Other\\demo.db";
                    if (!File.Exists($"{getProgramsFolder()}\\Other\\demo.db"))
                    {
                        new DemoDatabase().InstallDB(demoDBcs);
                    }
                    var connection = new SqliteConnection(demoDBcs);
                    return connection;
                }

                if (request.Level3Request() == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                if (request.Level3Request() == "IN.DB.DEMO") 
                {
                    var demoDBcs = $"Data Source={getProgramsFolder()}\\Other\\demo.db";
                    new DemoDatabase().QueryData(demoDBcs);
                    return true;
                }

                if (request.Context=="JSON.DESERIALIZE")
                {   // Context:JSON.DESERIALIZE, Group:text, Name:emp
                    var json = request.Interpreter.GetVar(request.Group).String;
                    return JsonSerializer.Deserialize(json,typeof(EmployeeProfile) );
                }

                if (request.Level3Request() == "IN.OBJECT.EMPLOYPROFILE")
                {
                    return new DemoObjects.EmployeeProfileExample().GetEmployeeProfile();
                }

                if (request.Level3Request() == "IN.OBJECT.SHOWEMPLOYPROFILE")
                {
                    var emp=request.Interpreter.GetVar(request.VariableName).Object;
                    if (emp is ExpandoObject)
                    {
                        emp = ExpandoMapper.MapToType<EmployeeProfile>((ExpandoObject)emp);
                    }
                    new DemoObjects.EmployeeProfileExample().TestPrint( (EmployeeProfile)emp);
                    return emp;
                }

                Console.WriteLine($"Interactive: THE REQUEST FOR OBJECT NOT FOUND. Context:{request.Context}, Group:{request.Group}, Name:{request.Name}");

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
            env.AddLibrary(new FBasicJsonLibrary());
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
