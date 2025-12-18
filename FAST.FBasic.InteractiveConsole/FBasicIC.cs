using FAST.FBasicInteractiveConsole;
using FAST.FBasicInterpreter;
using Microsoft.Extensions.Configuration;

namespace FAST.FBasic.InteractiveConsole
{
    internal partial class FBasicIC
    {
        private readonly IConfiguration config;
        private string iCommand;
        private string programsFolder;
        private string startupName;
        private ExecutionEnvironment env;

        string sourceProgram=null!;
        Interpreter basic= null!;
        ExecutionResult result = null!;
        bool asyncMode = true;

        public async Task run(string iCommandArg)
        {
            
            if (env == null ) setupEnvironment(); // run once
            iCommand = iCommandArg;

            switch (iCommand)
            {
                case "CLS":
                    ClearScreen();
                    break;

                case "TP": // test programs
                    new TPRunner().Run();
                    break;

                case "RUN":
                case "R":
                    await runFBasicProgram();
                    break;

                case "DUMP":
                case "D":
                    if (basic == null ) break;
                    basic.dumpInterpreter(true);

                    break;

                case "LIST":
                    Console.WriteLine($"LIST OF {this.startupName} LOADED ON THE LATEST RUN:");
                    Console.WriteLine( this.sourceProgram );
                    break;
             
                case "HELP":
                case "H":
                case "?":
                    help();
                    break;
                //case "B":
                //spBuilder();

                
                case "INFO":
                case "I":   
                    internalInfo();
                    break;

                case "DIR":
                    var files = Directory.GetFiles(programsFolder, "*.bas");
                    foreach(var fname in files)
                    {
                        Console.WriteLine(Path.GetFileName(fname));
                    }
                    break;
                
                case "LOAD":
                case "L":
                    while (true)
                    {
                        Console.Write("Enter program name to load (q to exit): ");
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

                case "SYNC":
                    if (asyncMode)
                        Console.WriteLine("Switching to SYNCHRONOUS mode");
                    else
                        Console.WriteLine("Switching to ASYNCHRONOUS mode");
                    asyncMode = !asyncMode;
                    break;

                case "S":
                    Console.WriteLine("REVERSED SOURCE:");
                    Console.WriteLine("---------------------------");
                    var file = Directory.GetFiles(programsFolder, startupName).FirstOrDefault();
                    var program = FBasicSource.ToProgram(file);
                    var src = FBasicSource.ToSource(program);
                    Console.WriteLine(src);
                    break;


                case "BC1":
                    ClearScreen();
                    Console.WriteLine("Demonstrating: Business Case 1:");
                    var BC1 = new BusinessCase1(this.programsFolder);
                    BC1.Scenario();
                    break;

                default:
                    break;
            }
            
        }

        private async Task runFBasicProgram()
        {
            
            var basProgramFile = Directory.GetFiles(programsFolder, startupName).FirstOrDefault();

            #region (+) Alternative way to execute a program
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
            #endregion

            #region (+) Other way to execute a program
            //result = FBasicSource.Run(env, basProgramFile);
            #endregion

            // (v) most analytical way to execute a program
            // 
            
            if (env == null) // in case the no environment, set the default environment. 
            {
                env = new();
                env.DefaultEnvironment(programsFolder);
            }
            this.sourceProgram = env.FileHandler( new FBasicFileDescriptor(){FileName= basProgramFile } ).GetSourceProgram();
            basic = new Interpreter(env.installBuiltIns, this.sourceProgram);
            env.SetupInterpreter(basic);

            if (asyncMode)
            {
                result = await basic.ExecWithResultAsync();
            }
            else
            {
                result = basic.ExecWithResult();
            }
            

            if (result.hasError)
            {
                Console.WriteLine(result.errorText);
                if (!string.IsNullOrEmpty(result.errorSourceLine)) Console.WriteLine(result.errorSourceLine);
            }
            else
            {
                Console.WriteLine();
                if (asyncMode)
                    Console.WriteLine("....................end of async program....................");
                else
                    Console.WriteLine("....................end of program....................");
                Console.WriteLine($"Result: {result.value}");
                Console.WriteLine();
            }
                

        }

    }
}
