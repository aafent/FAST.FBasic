using FAST.FBasicInterpreter.Core;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC
    /// </summary>
    public partial class Interpreter : IfbasicError
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
        /// <param name="value">output the value</param>
        /// <returns>True if the request found</returns>
        public delegate object RequestForObject(string context, string group, string name );

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
        public RequestForObject requestForObjectHandler = null;

        #endregion (+) Handlers

        #region (+) private working data properties & fields 

        private Token prevToken; // token before last one
        public Token lastToken; // last seen token

        private Dictionary<string, Value> vars; // all variables are stored here
        private Dictionary<string, Marker> labels; // already seen labels 
        private Dictionary<string, Marker> loops; // for loops
        private Stack<Marker> instructionStack; // for return points

        private Dictionary<string, BasicFunction> funcs; // all mapped functions
        private Dictionary<string, BasicStatement> statements; // all mapped statements
        private int ifCounter; // counter used for matching "if" with "else"
        private Marker lineMarker; // current line marker
        private bool exit; // do we need to exit?

        #endregion (+) private working data properties & fields 

        #region (+) public data properties & fields
        /// <summary>
        /// Logger for the execution
        /// </summary>
        executionLoggerAbstract log = null;
        /// <summary>
        /// The Lexer
        /// </summary>
        public Lexer lex;
        /// <summary>
        /// Data adapters 
        /// </summary>
        public readonly Dictionary<string, IfbasicDataAdapter> dataAdapters = new(); 
        /// <summary>
        /// Collection objects
        /// </summary>
        public readonly Dictionary<string, IfbasicCollection> collections = new();
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

        #region (+) Constructors

        /// <summary>
        /// Constructor with the source program as input
        /// </summary>
        /// <param name="source">the source</param>
        public Interpreter(bool installBuiltIns, string source)
        {
            commonConstructor(installBuiltIns, source);
        }

        /// <summary>
        /// Constructor with a program container as program
        /// </summary>
        /// <param name="program">the program</param>
        public Interpreter(bool installBuiltIns, programContainer program)
        {
            var source=fBasicHelper.toSource(program);
            commonConstructor(installBuiltIns, source);
        }

        private void commonConstructor(bool installBuiltIns, string source)
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
            this.log=new defaultExecutionLogger();

            if (installBuiltIns) 
            {
                this.AddLibrary(new BuiltIns());
                this.AddLibrary(new BuiltInsForCollections() );
            }
            this.AddLibrary( new FBasicStringFunctions());
        }

        #endregion (+) Constructors

    }
}
