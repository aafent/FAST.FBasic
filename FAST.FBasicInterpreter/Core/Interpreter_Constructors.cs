
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC
    /// Constructors
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) Constructors

        /// <summary>
        /// Simple constructor with only the build ins
        /// Invoke the SetSourceProgram() to place the source program
        /// </summary>
        /// <param name="installBuiltIns">true to install the build-ins</param>
        public Interpreter(bool installBuiltIns)
        {
            ResetInterpreter();
            constructorEnvironment(installBuiltIns);
        }

        /// <summary>
        /// Constructor with the source program as input
        /// </summary>
        /// <param name="source">the source</param>
        public Interpreter(bool installBuiltIns, string source)
        {
            ResetInterpreter();
            SetSourceProgram(source);
            constructorEnvironment(installBuiltIns);
        }

        /// <summary>
        /// Constructor with a program container as program
        /// </summary>
        /// <param name="program">the program</param>
        public Interpreter(bool installBuiltIns, ProgramContainer program)
        {
            var source=FBasicSource.ToSource(program);
            ResetInterpreter();
            SetSourceProgram(source);
            constructorEnvironment(installBuiltIns);
        }

        public void ResetInterpreter()
        {
            this.arrays =new();
            this.vars = new Dictionary<string, Value>();
            this.labels = new Dictionary<string, Marker>();
            this.loops = new Dictionary<string, Marker>();
            this.instructionStack = new();
            this.funcs = new Dictionary<string, FBasicFunction>();
            this.statementsSync = new();
            this.statementsAsync = new();
            this.ifCounter = 0;
            this.Result = new();
        }


        public void SetSourceProgram(string source)
        {
            this.lex = new Lexer(source, Error);
        }

        private void constructorEnvironment(bool installBuiltIns)
        {
            // (v) environment
            if (this.logger == null) this.logger = new defaultExecutionLogger();
            if (installBuiltIns)
            {
                this.AddLibrary(new BuiltIns());
                this.AddLibrary(new BuiltInsForCollections());
            }
        }

        #endregion (+) Constructors
    }
}
