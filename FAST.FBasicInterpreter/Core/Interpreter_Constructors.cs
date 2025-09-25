
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
        /// Constructor with the source program as input
        /// </summary>
        /// <param name="source">the source</param>
        public Interpreter(bool installBuiltIns, string source)
        {
            constructorCommon(source);
            constructorEnvironment(installBuiltIns);
        }

        /// <summary>
        /// Constructor with a program container as program
        /// </summary>
        /// <param name="program">the program</param>
        public Interpreter(bool installBuiltIns, ProgramContainer program)
        {
            var source=FBasicSource.ToSource(program);
            constructorCommon(source);
            constructorEnvironment(installBuiltIns);
        }

        private void constructorCommon(string source)
        {
            this.lex = new Lexer(source, Error);
            this.vars = new Dictionary<string, Value>();
            this.labels = new Dictionary<string, Marker>();
            this.loops = new Dictionary<string, Marker>();
            this.instructionStack = new();
            this.funcs = new Dictionary<string, BasicFunction>();
            this.statements = new();
            this.ifCounter = 0;
            this.Result = new();
        }

        private void constructorEnvironment(bool installBuiltIns)
        {
            // (v) environment
            this.log = new defaultExecutionLogger();
            if (installBuiltIns)
            {
                this.AddLibrary(new BuiltIns());
                this.AddLibrary(new BuiltInsForCollections());
            }
        }

        #endregion (+) Constructors
    }
}
