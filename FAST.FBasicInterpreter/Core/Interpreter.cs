
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) Handlers

        /// <summary>
        /// The delegation handler for the PRINT statement
        /// </summary>
        public FBasicPrintFunction printHandler=null;

        /// <summary>
        /// The delegation handler for the INPUT statement
        /// </summary>
        public FBasicInputFunction inputHandler=null;

        /// <summary>
        /// The delegation handler for the CALL statement
        /// </summary>
        public FBasicSourceProgramLoader callHandler = null;

        /// <summary>
        /// The delegation handler for Request for Object
        /// </summary>
        public FBasicRequestForObject requestForObjectHandler = null;

        /// <summary>
        /// Logger for the execution
        /// </summary>
        public IFBasicLogger logger = null;

        #endregion (+) Handlers

        #region (+) private working data properties & fields 

        private Token prevToken; // token before last one
        
        private Dictionary<string, Value> vars; // all variables are stored here
        private Dictionary<string, Marker> labels; // already seen labels 
        private Dictionary<string, Marker> loops; // for loops
        private Stack<Marker> instructionStack; // for return points

        private Dictionary<string, FBasicFunction> funcs; // all mapped functions
        private Dictionary<string, FBasicStatement> statements; // all mapped statements
        private int ifCounter; // (NOT WORKING, TO RECHECK) counter used for matching "if" with "else"
                            
        private Marker lineMarker; // current line marker
        private bool exit; // do we need to exit?

        #endregion (+) private working data properties & fields 

        #region (+) Public & Private data 

        /// <summary>
        /// Data adapters 
        /// </summary>
        public readonly Dictionary<string, IFBasicDataAdapter> dataAdapters = new();
        /// <summary>
        /// Collection objects
        /// </summary>
        public readonly Dictionary<string, IBasicCollection> collections = new();

        /// <summary>
        /// The result of the execution (statement: RESULT)
        /// </summary>
        public Value Result { get; set; }

        /// <summary>
        /// The GetNextToken handler, no need to be defined as the the interpreter uses the default built-in.
        /// </summary>
        public FBasicGetNextTokenMethod GetNextToken = null;

        #endregion (+) Public & Private data 

        #region (+) Parsing & Execution Elements

        /// <summary>
        /// Latest interpreted token
        /// </summary>
        public Token lastToken; // last seen token

        /// <summary>
        /// The Lexer
        /// </summary>
        public Lexer lex;

        /// <summary>
        /// Libraries with memory
        /// </summary>
        public Dictionary<string, IFBasicLibraryWithMemory> librariesWithMemory; // libraries with memory

        #endregion (+) Parsing & Execution Elements


    }
}
