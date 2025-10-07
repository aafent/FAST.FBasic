using FAST.FBasicInterpreter;
using Microsoft.Extensions.Configuration;
using System.Data.Odbc;

namespace FAST.FBasic.InteractiveConsole
{
    internal class FBasicIC
    {
        private readonly IConfiguration config;
        private string iCommand;
        private string programsFolder;
        private string startupName;
        private ExecutionEnvironment env;

        public FBasicIC(IConfiguration config)
        {
            this.iCommand = "RUN";
            this.config=config;
            this.programsFolder = config.GetValue<string>("Settings:ProgramsFolder")!;
            if (string.IsNullOrEmpty(programsFolder)) programsFolder= @"~\..\..\..\..\FAST.FBasicInterpreter\Tests";
            if (programsFolder.Contains("~")) 
                programsFolder = programsFolder.Replace("~", Environment.CurrentDirectory);

            this.startupName = config.GetValue<string>("Settings:Startup")!;
            if (string.IsNullOrEmpty(startupName)) startupName = "helloWorld.bas";
        }


        public void welcome()
        {
            Console.WriteLine("FBASIC Interpreter Interactive Console");
            Console.WriteLine("......................................");
        }

        public void help()
        {
            Console.WriteLine("Help on FBASIC test console");
            Console.WriteLine("---------------------------");
            Console.WriteLine("...not ready yet..... :-)  ");
        }

        public void testEventHandler1(object sender, (string name, Stack<Value> args) e)
        {
            Console.WriteLine($"1st Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
        }
        public void testEventHandler2(object sender, (string name, Stack<Value> args) e)
        {
            Console.WriteLine($"2nd Listener for event: {e.name} triggered. Stack contains {e.args.Count} arguments");
        }

        public void run(string iCommandArg)
        {
            if (env == null ) setupEnvironment(); // run once
            iCommand = iCommandArg;
            switch(iCommand)
            {
                case "R":
                case "RUN":
                    try
                    {
                        runFBasicProgram();
                    }
                    catch (FBasicException fbe)
                    {
                        Console.WriteLine(fbe.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Non FBASIC exception");
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                    
                    break;
                case "H":
                case "HELP":
                    help();
                    break;
                //case "B":
                //spBuilder();
                case "I":
                case "INFO":
                    internalInfo();
                    break;
                case "DIR":
                    var files = Directory.GetFiles(programsFolder, "*.bas");
                    foreach(var fname in files)
                    {
                        Console.WriteLine(Path.GetFileName(fname));
                    }
                    break;
                case "L":
                case "LOAD":
                    while (true)
                    {
                        Console.Write("Enter program name to load: ");
                        var pname=Console.ReadLine();
                        if (pname.ToUpper()=="Q") break;
                        if (!pname.ToUpper().EndsWith(".BAS")) pname+=".bas";
                        var pfile = Directory.GetFiles(programsFolder, pname).FirstOrDefault();
                        if (!string.IsNullOrEmpty(pfile))
                        {
                            this.startupName=pname;
                            Console.WriteLine($"Program name: {startupName}, enter R(UN) command to run it.");
                            break;
                        } 
                        else
                        {
                            Console.WriteLine($"Program: {pname} not found. Try again! (q=exit)");
                        }
                    }
                    break;

                case "S":
                    Console.WriteLine("REVERSED SOURCE:");
                    Console.WriteLine("---------------------------");
                    var file = Directory.GetFiles(programsFolder, startupName).FirstOrDefault();
                    var program = FBasicSource.ToProgram(file);
                    var src = FBasicSource.ToSource(program);
                    Console.WriteLine(src);
                    break;
                default:
                    break;
            }
            
        }

        private void setupEnvironment()
        {
            string cs = "<connection string>";
            //connectionAdapterForODBC connection = new(cs, dbDialectDetails.sql);

            this.env = new();
            env.DefaultEnvironment();
            env.callHandler = (name) => { var filepath = Path.Combine(programsFolder, name); return File.ReadAllText(filepath); };
            env.requestForObjectHandler = (context, group, name) =>
            {
                if ($"{context}.{group}.{name}" == "SQL.CONNECTION.DATA1") 
                {
                    string cs = "<replace with your CS hear>";
                    var connection = new OdbcConnection(cs);
                    return connection;
                }
                if ($"{context}.{group}.{name}" == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                return null;
            };
            env.AddLibrary(new FBasicStringFunctions());
            env.AddLibrary(new FBasicMathFunctions());
            env.AddLibrary(new FBasicDateFunctions());
            env.AddLibrary(new FBasicSQLDataAdapter());
            env.AddLibrary(new FBasicEvents());
            env.AddLibrary(new FBasicTextReplacer());
            env.AddLibrary(new FBasicStack());

            env.AddVariable("table.column", "myColumn1");

            FBasicEvents.Reset();
            FBasicEvents.FBasicEventHandler += testEventHandler1!;
            FBasicEvents.FBasicEventHandler += testEventHandler2!;
        }

        private void internalInfo()
        {
            Console.WriteLine($"Folder: {programsFolder}");
            Console.WriteLine($"Program: {startupName}");
        }

        private void runFBasicProgram()
        {
            ExecutionResult result;
            var basProgramFile = Directory.GetFiles(programsFolder, startupName).FirstOrDefault();

            /* Alternative way to execute a program
             * 
             * 
            result = FBasicSource.Run(env, basProgramFile, (interp) =>
            {
                interp.SetVar("table.column", new Value("myColumn1"));
                interp.AddLibrary(new FBasicStringFunctions());
                interp.AddLibrary(new FBasicMathFunctions());
                interp.AddLibrary(new FBasicDateFunctions());
                interp.AddLibrary(new FBasicSQLDataAdapter());
            });
            */

            result = FBasicSource.Run(env, basProgramFile);
            if (result.hasError)
            {
                Console.WriteLine(result.errorText);
                if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("....................end of program....................");
                Console.WriteLine($"Result: {result.value}");
            }
                

        }

        //    static void spBuilder()
        //    {
        //        var folder = Path.Combine(Environment.CurrentDirectory, @"..\..\..", "FBasicInterpreter", "Tests");
        //        Console.WriteLine($"Folder: {folder}");
        //        string name = "lets.bas";

        //        string cs = "<your CS here>";
        //        //connectionAdapterForODBC connection = new(cs, dbDialectDetails.sql);

        //        ExecutionEnvironment env = new();
        //        env.printHandler += Console.WriteLine;
        //        env.inputHandler += Console.ReadLine;
        //        env.callHandler += (name) => { var filepath = Path.Combine(folder, name); return File.ReadAllText(filepath); };
        //        env.requestForObjectHandler += (context, group, name) =>
        //        {
        //           // if ($"{context}.{group}.{name}" == "SQL.CONNECTION.ADAPTER") return connection;
        //            return null;
        //        };

        //        foreach (string file in Directory.GetFiles(folder, name))
        //        {
        //            Console.WriteLine("SP BUILDER:");
        //            Console.WriteLine("---------------------------");
        //            var program = FBasicSource.ToProgram(file);
        //            IsourceCodeBuilder builder = new storedProcedureBuilder();
        //            builder.Build(program);
        //            var src = builder.GetSource();
        //            Console.WriteLine(src);
        //        }
        //    }
    }
}
