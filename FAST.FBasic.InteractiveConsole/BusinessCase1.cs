using FAST.FBasicInterpreter;
using FAST.FBasicInterpreter.Execution;

namespace FAST.FBasic.InteractiveConsole
{
    internal class BusinessCase1
    {
        private readonly string programsFolder;

        public BusinessCase1(string programsFolder)
        {
            this.programsFolder = programsFolder;
        }   


        public void Scenario()
        {
            Console.WriteLine("Business Case 1 - Credit Scoring");

            DateTime dateOfBirth = new DateTime(1985, 5, 7);
            int missedPayments = 2;
            int creditUtilRatio = 60;
            int yearsOfCreditHistory = 7;
            int yearsAtEmployer = 4;

            Console.WriteLine($"Date of Birth            : {dateOfBirth:dd/MM/yyyy}");
            Console.WriteLine($"Missed Payments          : {missedPayments}");
            Console.WriteLine($"Credit Utilization Ratio : {creditUtilRatio}");
            Console.WriteLine($"Years of Credit History  : {yearsOfCreditHistory}");
            Console.WriteLine($"Years at current employer: {yearsAtEmployer}");
            Console.WriteLine();

            (int score,string decision)= Score(dateOfBirth, missedPayments,creditUtilRatio,yearsOfCreditHistory,yearsAtEmployer);

            Console.WriteLine($"Score & Decision: {score} : {decision}");

        }

        public (int,string) Score(DateTime dateOfBirth, 
                                    int missedPayments, int creditUtilRatio, 
                                    int yearsOfCreditHistory, int yearsAtEmployer)
        {
            ArgumentsToInput args = new ArgumentsToInput(dateOfBirth, missedPayments, creditUtilRatio, yearsOfCreditHistory, yearsAtEmployer);
            var result = Run("score",args);
            var decisionText=result.GetStringVariable("decisionText");
            return (result.value.ToInt(), decisionText);
        }



        protected ExecutionResult Run(string programName, ArgumentsToInput args=null)
        {
            var env= SetupExecutionEnvironment();
            if (args != null)
            {
                env.inputHandler = () => args.GetNextArgumentAsInput();
            }
            var scoreProgram = File.ReadAllText(Directory.GetFiles(programsFolder, $"{programName}.bas").FirstOrDefault()!);
            var interpreter = new Interpreter(env.installBuiltIns, scoreProgram);
            env.SetupInterpreter(interpreter);
            ExecutionResult result = interpreter.ExecWithResult();
            if (result.hasError)
            {
                throw new Exception($"{result.errorText}\n{result.errorSourceLine}" );
            }
            return result;
        }

        protected ExecutionEnvironment SetupExecutionEnvironment()
        {
            ExecutionEnvironment env= new ExecutionEnvironment();

            env.DefaultEnvironment();
            env.callHandler = (name) => File.ReadAllText(Path.Combine(this.programsFolder, name));

            env.AddLibrary(new FBasicDateFunctions());
            env.AddLibrary(new FBasicDecisionTables());

            //env.AddLibrary(new FBasicStringFunctions());
            //env.AddLibrary(new FBasicMathFunctions());
            //env.AddLibrary(new FBasicSQLDataAdapter());
            //env.AddLibrary(new FBasicEvents());
            //env.AddLibrary(new FBasicTextReplacer());
            //env.AddLibrary(new FBasicStack());

            return env;
        }


    }


}
