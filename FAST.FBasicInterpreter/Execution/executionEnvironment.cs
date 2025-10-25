namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The base environment of an FBASIC Program
    /// Contains the necessary context to run a program
    /// </summary>
    public class ExecutionEnvironmentBase
    {
        #region (+) Constructors
        /// <summary>
        /// No parameters constructor
        /// </summary>
        public ExecutionEnvironmentBase()
        {
        }

        /// <summary>
        /// Construct the environment from a base environment object
        /// </summary>
        /// <param name="baseEnvironment">Execution environment</param>
        public ExecutionEnvironmentBase(ExecutionEnvironmentBase environmentToCopy) : this()
        {
            this.printHandler = environmentToCopy.printHandler;
            this.inputHandler = environmentToCopy.inputHandler;
            this.callHandler = environmentToCopy.callHandler;
            this.requestForObjectHandler = environmentToCopy.requestForObjectHandler;
            this.executionLogger = environmentToCopy.executionLogger;
            
            this.installBuiltIns = environmentToCopy.installBuiltIns;

            this.libraries = environmentToCopy.libraries;
            this.variables = environmentToCopy.variables;

        }
        #endregion (+) Constructors

        #region (+) Handlers
        /// <summary>
        /// The handler for PRINT statement
        /// </summary>
        public FBasicPrintFunction printHandler { get; set; } = null;

        /// <summary>
        /// The handler for the INPUT statement 
        /// </summary>
        public FBasicInputFunction inputHandler { get; set; } = null;

        /// <summary>
        /// The handler for the CALL statement
        /// This used to load the source of the calling program
        /// </summary>
        [Obsolete("Use the FileHandler instead")]
        public FBasicSourceProgramLoader callHandler = null;

        /// <summary>
        /// The file handler. 
        /// For source program and in-program files
        /// </summary>
        public FBasicFileManagement FileHandler { get; set; } = null;

        /// <summary>
        /// The handler of Request For Object mechanism 
        /// </summary>
        public FBasicRequestForObject requestForObjectHandler = null;

        #endregion (+) Handlers

        /// <summary>
        /// Libraries
        /// </summary>
        public List<IFBasicLibrary> libraries {get; set;}

        /// <summary>
        /// Variables
        /// </summary>
        public Dictionary<string,Value> variables { get; set; }


        #region (+) Logging class
        /// <summary>
        /// The execution logger
        /// </summary>
        public FBasicLoggerAbstract executionLogger = new defaultExecutionLogger();
        #endregion (+) Logging class

        #region (+) Working switches 
        /// <summary>
        /// Flag to disable the installation of Built-Ins statements and functions.
        /// </summary>
        public bool installBuiltIns { get; set; } = true;

        #endregion (+) Working switches 

    }


    /// <summary>
    /// The execution environment of an FBASIC Program
    /// </summary>
    public class ExecutionEnvironment : ExecutionEnvironmentBase
    {
        #region (+) Constructors
        /// <summary>
        /// No parameters constructor
        /// </summary>
        public ExecutionEnvironment():base()
        {
        }

        /// <summary>
        /// Construct the environment from a base environment object
        /// </summary>
        /// <param name="baseEnvironment">Execution environment</param>
        public ExecutionEnvironment(ExecutionEnvironmentBase environmentToCopy) :base(environmentToCopy)
        {
        }
        #endregion (+) Constructors

        /// <summary>
        /// Add libraries to execution environment
        /// </summary>
        /// <param name="library">Library instance to add</param>
        public void AddLibrary(IFBasicLibrary library)  
        {
            if (libraries == null) libraries = new();
            libraries.Add(library);
        }

        #region (+) add variables 
        /// <summary>
        /// Add value to a variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddVariable(string name, Value value)
        {
            if (variables==null) variables=new();
            if (variables.ContainsKey(name) )
            {
                variables[name]=value;
            }
            else
            {
                variables.Add(name, value);
            }
        }
        public void AddVariable(string name, double value) => AddVariable(name,new Value(value));
        public void AddVariable(string name, int value) => AddVariable(name, new Value(value));
        public void AddVariable(string name, string value) => AddVariable(name, new Value(value));
        #endregion (+) add variables 

        /// <summary>
        /// Setup Interpreter by adding libraries and variables
        /// </summary>
        /// <param name="interpreter"></param>
        public void SetupInterpreter(IInterpreter interpreter)
        {
            // (v) setup handlers
            if (this.printHandler!=null) interpreter.printHandler=this.printHandler;
            if (this.inputHandler!=null) interpreter.inputHandler=this.inputHandler;
            if (this.callHandler!=null) interpreter.callHandler=this.callHandler;
            if (this.FileHandler!=null) interpreter.FileHandler=this.FileHandler;
            if (this.requestForObjectHandler!=null) interpreter.requestForObjectHandler=this.requestForObjectHandler;
            if (this.executionLogger!=null) interpreter.logger = this.executionLogger;


            // (v) install variables
            if (this.variables != null)
            {
                foreach (var variable in this.variables)
                {
                    interpreter.SetVar(variable.Key, variable.Value);
                }
            }

            // (v) install libraries
            if (this.libraries != null)
            {
                foreach (var library in this.libraries)
                {
                    interpreter.AddLibrary(library);
                }
            }
        }
    }


    /// <summary>
    /// Extensions to the ExecutionEnvironment class
    /// </summary>
    public static class ExecutionEnvironment_Extensions 
    {
        /// <summary>
        /// set a default working environment using the Console as input, output
        /// the current directory as call handler and empty (all null) request for object handler.
        /// Use this code as example of initialization.
        /// </summary>
        /// <returns></returns>
        public static void DefaultEnvironment(this ExecutionEnvironment env, string rootFolder=null)
        {
            env.printHandler = Console.Write;
            env.inputHandler = Console.ReadLine;
            env.callHandler = (name) => { var filepath = Path.Combine(".", name); return File.ReadAllText(filepath); };
            env.requestForObjectHandler = (context, group, name) => null;

            if (!string.IsNullOrEmpty(rootFolder) )
            {
                env.FileHandler = new DefaultFileManagementLayer(rootFolder).Handler;
            }
            

            return;
        }

    }

}
