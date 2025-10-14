
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) Handlers

        /// <summary>
        /// The delegation type of the  printHandler
        /// </summary>
        /// <param name="text">The the to PRINT</param>
        public delegate void PrintFunction(string text);

        /// <summary>
        /// The delegation type of the input handler
        /// </summary>
        /// <returns>The INPUT</returns>
        public delegate string InputFunction();

        /// <summary>
        /// The delegation type of the load source for the CALL statement
        /// </summary>
        /// <param name="name">the name of the source program</param>
        /// <returns>the source</returns>
        public delegate string CallStatementLoader(string name);

        /// <summary>
        /// The delegation of object requesting
        /// </summary>
        /// <param name="context">The Context</param>
        /// <param name="group">The group of names</param>
        /// <param name="name">The name</param>
        /// <returns>object</returns>
        public delegate object RequestForObjectAction(string context, string group, string name );

        /// <summary>
        /// The delegation handler for the PRINT statement
        /// </summary>
        public PrintFunction printHandler=null;

        /// <summary>
        /// The delegation handler for the INPUT statement
        /// </summary>
        public InputFunction inputHandler=null;

        /// <summary>
        /// The delegation type of the  logHandler
        /// </summary>
        /// <param name="text">The the to PRINT</param>
        public delegate void LogFunction(string text);


        /// <summary>
        /// The delegation handler for the CALL statement
        /// </summary>
        public CallStatementLoader callHandler = null;

        /// <summary>
        /// The delegation handler for Request for Object
        /// </summary>
        public RequestForObjectAction requestForObjectHandler = null;

        #endregion (+) Handlers

        #region (+) private working data properties & fields 

        private Token prevToken; // token before last one
        
        private Dictionary<string, Value> vars; // all variables are stored here
        private Dictionary<string, Marker> labels; // already seen labels 
        private Dictionary<string, Marker> loops; // for loops
        private Stack<Marker> instructionStack; // for return points

        private Dictionary<string, BasicFunction> funcs; // all mapped functions
        private Dictionary<string, BasicStatement> statements; // all mapped statements
        private int ifCounter; // (NOT WORKING, TO RECHECK) counter used for matching "if" with "else"
                            
        private Marker lineMarker; // current line marker
        private bool exit; // do we need to exit?

        #endregion (+) private working data properties & fields 

        #region (+) public data properties & fields
        /// <summary>
        /// Latest interpreted token
        /// </summary>
        public Token lastToken; // last seen token
        /// <summary>
        /// Logger for the execution
        /// </summary>
        public IFBasicLogger log = null;
        /// <summary>
        /// The Lexer
        /// </summary>
        public Lexer lex;
        /// <summary>
        /// Data adapters 
        /// </summary>
        public readonly Dictionary<string, IFBasicDataAdapter> dataAdapters = new(); 
        /// <summary>
        /// Collection objects
        /// </summary>
        public readonly Dictionary<string, IBasicCollection> collections = new();

        /// <summary>
        /// Libraries with memory
        /// </summary>
        public Dictionary<string, IFBasicLibraryWithMemory> librariesWithMemory; // libraries with memory

        /// <summary>
        /// The result of the execution (statement: RESULT)
        /// </summary>
        public Value Result { get; set; }
        #endregion (+) public data properties & fields

        #region (+) Element delegates

        /// <summary>
        /// A new function delegate
        /// </summary>
        /// <param name="interpreter">The instance of this interpreter</param>
        /// <param name="args">Arguments</param>
        /// <returns>The function result</returns>
        public delegate Value BasicFunction(Interpreter interpreter, List<Value> args);

        /// <summary>
        /// A new statement delegate
        /// </summary>
        /// <param name="interpreter">The instance of this interpreter</param>
        public delegate void BasicStatement(Interpreter interpreter);

        /// <summary>
        /// Delegate type for GetNextToken Method
        /// For future use
        /// </summary>
        /// <returns>Token</returns>
        public delegate Token getNextTokenMethod();
        /// <summary>
        /// The GetNextToken handler
        /// </summary>
        public getNextTokenMethod GetNextToken = null;
        
        #endregion (+) Element delegates

    }
}
