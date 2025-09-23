using static FAST.FBasicInterpreter.Interpreter;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The base environment of an FBASIC Program
    /// Contains the necessary context to run a program
    /// </summary>
    public class executionEnvironmentBase
    { 
    }


    /// <summary>
    /// The execution environment of an FBASIC Program
    /// </summary>
    public class executionEnvironment : executionEnvironmentBase
    {

        /// <summary>
        /// No parameters constructor
        /// </summary>
        public executionEnvironment()
        {
        }

        /// <summary>
        /// Construct the environment from a base environment object
        /// </summary>
        /// <param name="baseEnvironment">Execution environment</param>
        public executionEnvironment(executionEnvironmentBase baseEnvironment) :this()
        {
        }


        /// <summary>
        /// The handler for PRINT statement
        /// </summary>
        public PrintFunction printHandler {  get; set; } = null;

        /// <summary>
        /// The handler for the INPUT statement 
        /// </summary>
        public InputFunction inputHandler { get; set; } = null;

        /// <summary>
        /// The handler for the CALL statement
        /// This used to load the source of the calling program
        /// </summary>
        public CallStatementLoader callHandler = null;

        /// <summary>
        /// The handler of Request For Object mechanism 
        /// </summary>
        public RequestForObject requestForObject=null;

        /// <summary>
        /// The execution logger
        /// </summary>
        public FBasicLoggerAbstract executionLogger = new defaultExecutionLogger();

        /// <summary>
        /// Flag to disable the installation of Built-Ins statements and functions.
        /// </summary>
        public bool installBuiltIns { get; set; } = true;


        /// <summary>
        /// Get a default working environment using the Console as input, output
        /// the current directory as call handler and empty (all null) request for object handler.
        /// Use this code as example of initialization.
        /// </summary>
        /// <returns></returns>
        public static executionEnvironment DefaultEnvironment()
        {
            executionEnvironment env = new();
            env.printHandler += Console.WriteLine;
            env.inputHandler += Console.ReadLine;
            env.callHandler += (name) => { var filepath = Path.Combine(".", name); return File.ReadAllText(filepath); };
            env.requestForObject += (context, group, name) =>
            {
                //if ($"{context}.{group}.{name}" == "SQL.CONNECTION.ADAPTER") return connection;
                return null;
            };

            return env;
        }


    }
}
