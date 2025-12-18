using Microsoft.Extensions.Configuration;

namespace FAST.FBasic.InteractiveConsole
{
    internal partial class FBasicIC
    {
        public FBasicIC(IConfiguration config)
        {
            this.iCommand = "RUN";
            this.config=config;
            this.programsFolder=getProgramsFolder();
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
            Console.WriteLine("RUN  | R  :: Run the last loaded program");
            Console.WriteLine("LOAD | L  :: Load a program from the tests folder");
            Console.WriteLine("INFO | I  :: Get information about the execution, errors, etc");
            Console.WriteLine("LIST |    :: List the last loaded program");
            Console.WriteLine("DIR  |    :: List all .BAS files in the tests folder");
            Console.WriteLine("S    |    :: Show the reversed source of the last loaded program");
            Console.WriteLine("DUMP | D  :: Dump the interpreter state");
            Console.WriteLine("HELP | ?  :: Show this help");
            Console.WriteLine("QUIT | Q  :: Exit of the test console program");
            Console.WriteLine("CLS  |    :: Clear the screen");
            Console.WriteLine("SYNC |    :: switch between async and sync mode");
            Console.WriteLine();
            Console.WriteLine("BC1  |    :: Run Business Case 1 - Credit Scoring");
            Console.WriteLine();
            Console.WriteLine("TP   |    :: Run Experiment and test programs. Make sense only to the developers of the FBasic");

        }

        public void ClearScreen()
        {
            //for (int i = 0; i < 50; i++) Console.WriteLine();
            Console.Clear();
        }




        private void internalInfo()
        {
            Console.WriteLine($"Folder: {programsFolder}");
            Console.WriteLine($"Program: {startupName}");
            if (result!=null)
            {
                var diff= result.programEndWhen - result.programStartWhen;
                Console.WriteLine($"Run duration: {diff.ToString("c")} seconds");
            }
            Console.WriteLine();
            #if DEBUG
            if (result!=null && result.exception!= null)
            {
                Console.WriteLine($"Exception:");
                string modifiedTrace = StackTraceProcessor.ReplaceFilePathWithFileName(result.exception.StackTrace);
                Console.WriteLine(modifiedTrace);
            }
            #endif
        }


        private string getProgramsFolder()
        {
            var progFolder = config.GetValue<string>("Settings:ProgramsFolder")!;
            if (string.IsNullOrEmpty(progFolder)) progFolder = @"~\\..\\..\\..\\..\\FAST.FBasic.InteractiveConsole\\Tests";
            if (progFolder.Contains("~"))
                progFolder = progFolder.Replace("~", Environment.CurrentDirectory);
            //return progFolder;
            return Path.GetFullPath(progFolder);
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
