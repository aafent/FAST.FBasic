namespace FAST.FBasicInterpreter
{
    public interface IInterpreter
    {
        Value Result { get; set; }
        public Token LastToken { get; }
        IInterpreterLexer lex { get; set; }
        Dictionary<string, IFBasicLibraryWithMemory> librariesWithMemory { get; set; }


        #region (+) Add elements methods

        void AddArray(string name, FBasicArray array);
        void AddCollection(string name, IBasicCollection collection);
        void AddDataAdapter(IFBasicDataAdapter adapter);
        void AddFunction(string name, FBasicFunction function);
        void AddStatement(string name, FBasicStatement statement);

        #endregion (+) Add elements methods


        #region (+) Get methods
        InterpretationState GetState();
        FBasicArray GetArray(string name);
        IBasicCollection GetCollection(string name);
        Marker GetCurrentInstructionMarker();
        T GetDataAdapter<T>(string name) where T : IFBasicDataAdapter;
        Value GetVar(string name);
        Dictionary<string, Value> GetVariables();
        Value ValueOrVariable(bool doMatch=false);
        Value GetIdentifierOrCF(bool permitIdentifier = true, bool permitCollection = true, bool permitFunc = true);

        /// <summary>
        /// Get a value of any name (variable, collection item)
        /// </summary>
        /// <param name="interpreter">the interpreter</param>
        /// <param name="name">The name</param>
        /// <returns>Value</returns>
        Value GetValue(string name);

        #endregion (+) Get methods

        #region (+) Is methods 
        bool IsArray(string name);
        bool IsCollection(string name);
        bool IsDataAdapter(string name);
        bool IsFunction(string name);
        bool IsVariable(string name);

        #endregion (+) Is methods

        #region (+) Parsing Source Code methods
        FBasicGetNextTokenMethod GetNextToken { get; }
        Value Expr(int min = 0);
        
        void GoToInstructionMarker(Marker whereToGo);
        void JumpToNextToken(Token tok);
        void Match(Token tok);
        Marker PushCurrentInstructionMarker();
        void PushInstructionMarker(Marker marker);

        #endregion (+) Parsing Source Code methods


        #region (+) Set methods
        bool SetFlowToLabelIfExists(string label);
        void SetLastTokenToNewLine();
        void SetSourceProgram(string source);
        void SetState(InterpretationState state);
        void SetVar(string name, Value val);
        #endregion (+) Set methods

        #region (+) Program execution methods
        void ResetInterpreter();
        void RestartProgram();
        ErrorReturnClass Error(string source, string text);
        void Exec();
        #endregion (+) Program execution methods

        object RequestForObject(string context, string group, string name, bool errorIfNull = true);

        void dumpInterpreter(bool interactive = false);
        bool TryParseSourceCode(out ProgramContainer program);
        
        #region (+) Handlers

        /// <summary>
        /// The delegation handler for the PRINT statement
        /// </summary>
        public FBasicPrintFunction printHandler {get; set; }

        /// <summary>
        /// The delegation handler for the INPUT statement
        /// </summary>
        public FBasicInputFunction inputHandler { get;set;}

        /// <summary>
        /// The delegation handler for the CALL statement
        /// </summary>
        [Obsolete("Use the FileHandler instead")]
        public FBasicSourceProgramLoader callHandler { get; set; }

        /// <summary>
        /// The file handler. 
        /// For source program and in-program files
        /// </summary>
        public FBasicFileManagement FileHandler { get; set; } 

        /// <summary>
        /// The delegation handler for Request for Object
        /// </summary>
        public FBasicRequestForObject requestForObjectHandler { get; set; }

        /// <summary>
        /// Logger for the execution
        /// </summary>
        public IFBasicLogger logger { get; set; }

        


        #endregion (+) Handlers


    }
}