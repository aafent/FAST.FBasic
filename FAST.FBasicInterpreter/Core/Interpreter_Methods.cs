namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC
    /// Part: THE FBASIC INTERPRETER METHODS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) Add and more for variables

        /// <summary>
        /// Set a variable value
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="val">The value</param>
        public void SetVar(string name, Value val)
        {
            if (funcs.ContainsKey(name))
            {
                Error($"{name} is a function, cannot be declared as variable name [E117].");
            }
            if (!vars.ContainsKey(name)) vars.Add(name, val);
            else vars[name] = val;
        }

        /// <summary>
        /// Check if a variable is declared or not
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <returns>True,if is declared</returns>
        public bool IsVariable(string name)
        {
            return vars.ContainsKey(name);
        }

        /// <summary>
        /// Check if an array is declared
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsArray(string name)
        {
            return this.arrays.ContainsKey(name);
        }

        /// <summary>
        /// Check if a collection is declared
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsCollection(string name)
        {
            return this.collections.ContainsKey(name);  
        }


        /// <summary>
        /// Get reference to a declared Collection
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IBasicCollection GetCollection(string name)
        {
            if (!IsCollection(name))
            {
                Error(Errors.E112_UndeclaredEntity("Collection", name));
                return null; // unnecessary but the compiler will like it...
            }
            return this.collections[name];
        }

        /// <summary>
        /// Get reference to a declared array 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FBasicArray GetArray(string name)
        {
            if (!IsArray(name) )
            {
                Error(Errors.E112_UndeclaredEntity("Array",name));
                return null; // unnecessary but the compiler will like it...
            }
            return this.arrays[name];
        }

        /// <summary>
        /// Add new array to arrays
        /// </summary>
        /// <param name="name"></param>
        /// <param name="array"></param>
        public void AddArray(string name, FBasicArray array)
        {
            if (IsArray(name))
            {
                Error(Errors.E133_AlreadyDefined("Array", name));
                return; // unnecessary but the compiler will like it...
            }
            this.arrays.Add(name, array);
        }


        /// <summary>
        /// Get underlying variables and values 
        /// </summary>
        /// <returns>Dictionary, the key is the variable name</returns>
        public Dictionary<string, Value> GetVariables()
        {
            return this.vars;
        }


        /// <summary>
        /// Get a value of any name (variable, collection item)
        /// </summary>
        /// <param name="interpreter">the interpreter</param>
        /// <param name="name">The name</param>
        /// <returns>Value</returns>
        public Value GetValue(string name)
        {
            var parser = new IdentifierNotationParser(name, this);
            if (parser.IsArray)
            {
                return this.GetArray(parser.DataContainerName)[parser.ArrayIndex, parser.DataElement];
            }
            else if (parser.IsCollection)
            {
                return this.collections[parser.DataContainerName].getValue(parser.DataElement);
            }
            else
            {
                return this.GetVar(name);
            }

        }

        // (>) GetVar is at Program execution & control region. 

        #endregion (+) Add and more for variables

        #region (+) Add elements

        /// <summary>
        /// Add a new function
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="function">A reference to the function implementation</param>
        public void AddFunction(string name, FBasicFunction function)
        {
            if (!funcs.ContainsKey(name)) funcs.Add(name, function);
            else funcs[name] = function;
        }

        /// <summary>
        /// Add a new statement
        /// </summary>
        /// <param name="name">The name of the statement</param>
        /// <param name="statment">A reference to the statement method implementation</param>
        public void AddStatement(string name, FBasicStatement statement)
        {
            name = name.ToUpper(); // crucial to be upper
            if (!statements.ContainsKey(name)) statements.Add(name, statement);
            else statements[name] = statement;

        }

        /// <summary>
        /// Add data adapter. 
        /// Adapter can added only once
        /// </summary>
        /// <param name="adapter">The adapter (IfbasicDataAdapter) </param>
        public void AddDataAdapter(IFBasicDataAdapter adapter)
        {
            if (!dataAdapters.ContainsKey(adapter.name))
            {
                dataAdapters.Add(adapter.name, adapter);
                adapter.bind(this);
            }
            else throw new Exception($"Data adapter: ${adapter.name} exists. Cannot add more than once. [X100]");
        }

        /// <summary>
        /// Check by name if a data adapter is declared 
        /// </summary>
        /// <param name="name">Adapter name</param>
        /// <returns>Boolean, true if exists</returns>
        public bool IsDataAdapter(string name)
        {
            return dataAdapters.ContainsKey(name);
        }

        /// <summary>
        /// Get Data Adapter by name
        /// </summary>
        /// <typeparam name="T">The type of the adapter</typeparam>
        /// <param name="name">The name of the adapter</param>
        /// <returns>The instance of the Adapter</returns>
        public T GetDataAdapter<T>(string name) where T : IFBasicDataAdapter
        {
            return (T)dataAdapters[name];
        }

        /// <summary>
        /// Add or Set collection to the interpreter
        /// </summary>
        /// <param name="name">The collection name</param>
        /// <param name="collection">The collection handler</param>
        public void AddCollection(string name, IBasicCollection collection)
        {
            if (!dataAdapters.ContainsKey(name))
            {
                collections.Add(name, collection);
            }
            else
            {
                collections[name] = collection;
            }
        }

        /// <summary>
        /// Request For Object
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="group">The group</param>
        /// <param name="name">The name</param>
        /// <returns>The requested object or null</returns>
        public object RequestForObject(IFBasicRequestForObjectDescriptor descriptor, bool errorIfNull = true)
        {
            if (this.RequestForObjectHandler == null)
            {
                Error(descriptor.Context, Errors.E100_RequestForObjectHandlerNotInstalled(descriptor));
                return null;
            }
            var returnObject = this.RequestForObjectHandler(descriptor);
            if (errorIfNull & (returnObject == null))
                Error(descriptor.Context, $"Cannot get object for: ({descriptor.Context},{descriptor.Group},{descriptor.Name}) [E101]");
            return returnObject;
        }

        /// <summary>
        /// Check if a name is function
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsFunction(string name)
        {
            return funcs.ContainsKey(name);
        }

        #endregion (+) Add elements 

        #region (+) Error handling

        /// <summary>
        /// Raise an error
        /// </summary>
        /// <param name="source">The source of the error</param>
        /// <param name="text">The message</param>
        public ErrorReturnClass Error(string source, string text)
        {
            if (string.IsNullOrEmpty(source))
            {
                return Error($"{text}");
            }
            else
            {
                return Error($"{source}: {text}");
            }

        }
        private ErrorReturnClass Error(string text)
        {
            if (lineMarker.Line > 0)
            {
                string line = lex.GetLine(lineMarker.Line);
                if (!string.IsNullOrEmpty(line))
                {
                    if (lineMarker.Column >= 0 && lineMarker.Column <= line.Length)
                    {
                        line = line.Insert(lineMarker.Column, "<--E  ");
                    }
                }
                throw new FBasicException(text, lineMarker, line);
            }
            else
            {
                throw new FBasicException(text, lineMarker);
            }
        }

        #endregion (+) Error handling

        #region (+) Parsing & navigation methods

        /// <summary>
        /// Get next line
        /// </summary>
        /// <returns>A string with the line</returns>
        //public string GetLine()
        //{
        //    return lex.GetLine(lineMarker);
        //}

        /// <summary>
        /// Check the last found token if it is the requested.
        /// </summary>
        /// <param name="tok">The token</param>
        public void Match(Token tok)
        {
            // check if current token is what we expect it to be
            if (lastToken != tok)
                Error(Errors.E102_ExpectingThatButGotThis(tok.ToString(), lastToken.ToString()));
        }

        /// <summary>
        /// Jump to next token 
        /// </summary>
        /// <param name="tok">The Token to jump</param>
        public void JumpToNextToken(Token tok)
        {
            while (lastToken != Token.EOF)
            {
                if (GetNextToken() == tok)
                {
                    return;
                }
                if (lastToken == Token.EOF)
                {
                    Error($"Cannot jump to next token {tok.ToString()} [E115]");
                }
            } // while loop
            Error($"EOF found trying to jump to next token {tok.ToString()} [E116]");
        }

        /// <summary>
        /// Set the last token as NewLine;
        /// </summary>
        public void SetLastTokenToNewLine()
        {
            lastToken = Token.NewLine;
            lex.SetLastChar('\r');
        }

        /// <summary>
        /// set the programs flow to a label, if exists
        /// if the label is missing, the instruction pointer remains as it was
        /// </summary>
        /// <param name="label"></param>
        /// <returns>true if the label found.</returns>
        public bool SetFlowToLabelIfExists(string label)
        {
            var currentMarket = lex.CurrentSourceMarker; // save the position
            if (!labels.ContainsKey(label))
            {
                // if we didn't encounter required label yet, start to search for it
                while (true)
                {
                    if (GetNextToken() == Token.Colon && prevToken == Token.Identifier)
                    {
                        if (!labels.ContainsKey(lex.Identifier))
                            labels.Add(lex.Identifier, lex.CurrentSourceMarker);
                        if (lex.Identifier == label)
                            break;
                    }
                    if (lastToken == Token.EOF)
                    {
                        lex.GoTo(currentMarket); // restore the position
                        return false;
                    }
                }
            }
            lex.GoTo(labels[label]); // position will changed here, no need to restore
            SetLastTokenToNewLine();
            return true;
        }

        /// <summary>
        /// Save current Instruction point to Instruction stack
        /// </summary>
        /// <returns>Marker that pushed</returns>
        public Marker PushCurrentInstructionMarker()
        {
            instructionStack.Push(lex.CurrentSourceMarker);
            return lex.CurrentSourceMarker;
            //instructionStack.Push(lex.TokenMarker);
            //return lex.TokenMarker;
        }

        /// <summary>
        /// Push to the stack the specified marker
        /// </summary>
        /// <param name="marker"></param>
        public void PushInstructionMarker(Marker marker)
        {
            instructionStack.Push(marker);
        }

        /// <summary>
        /// Get the current instruction marker
        /// </summary>
        /// <returns></returns>
        public Marker GetCurrentInstructionMarker()
        {
            return lex.CurrentSourceMarker;
            //return lex.TokenMarker;
        }

        public void GoToInstructionMarker(Marker whereToGo)
        {
            lex.GoTo(whereToGo);
        }

        private Token interpreter_GetNextToken()
        {
            prevToken = lastToken;
            lastToken = lex.GetToken();


            if (lastToken == Token.EOF && prevToken == Token.EOF)
            {
                Error($"Unexpected end of program [E103]");
            }
                

            return lastToken;
        }

        #endregion (+) Parsing & navigation methods

        #region (+) Program execution & control 

        /// <summary>
        /// Get a variable value
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <returns>The value</returns>
        public Value GetVar(string name)
        {
            if (!vars.ContainsKey(name))
                throw new FBasicException($"Variable with name {name} does not exist. [X101]", lineMarker.Line);
            return vars[name];
        }

        /// <summary>
        /// Execute the program
        /// </summary>
        public void Exec()
        {
            if (this.librariesWithMemory != null)
            {
                foreach (var lib in this.librariesWithMemory.Values)
                {
                    lib.PrepareToExecute();
                }
            }
            lex.SetAddonStatements(statements.Keys.ToArray());
            GetNextToken = interpreter_GetNextToken;
            exit = false;
            GetNextToken();
            while (!exit) Line(); // do all lines
        }

        /// <summary>
        /// Restart a program before re-execute it
        /// </summary>
        public void RestartProgram()
        {
            this.lex.RestartProgram();
            this.Result = new();
            this.instructionStack = new();
            this.ifCounter = 0;
            // (!) check if we need more "resets" here. Check the commonConstructor()

            if (this.librariesWithMemory != null)
            {
                foreach (var lib in this.librariesWithMemory.Values)
                {
                    lib.ClearMemory();
                }
            }
        }

        /// <summary>
        /// Create a program container
        /// </summary>
        /// <returns>The program container</returns>
        public bool TryParseSourceCode(out ProgramContainer program)
        {
            program = new();
            List<ProgramElement> src = new();
            program.variables = new();
            try
            {
                GetNextToken = interpreter_GetNextToken;
                exit = false;
                GetNextToken();
                while (!exit)
                {
                    // (v) keep one new line and skip more empty new lines
                    if (lastToken == Token.NewLine)
                    {
                        if (src.Count > 0) // skip all the first newlines if the program start with them
                        {
                            src.Add(new ProgramElement() { token = lastToken, value = lex.Value, identifier = lex.Identifier });
                        }
                        while (lastToken == Token.NewLine) GetNextToken();
                    }
                    if (lastToken == Token.EOF)
                    {
                        exit = true;
                        break;
                    }
                    lineMarker = lex.CurrentSourceMarker; // save current line marker
                    src.Add(new ProgramElement()
                    {
                        token = lastToken,
                        value = lex.Value,
                        identifier = lex.Identifier,
                        isDoted = lex.Identifier.Contains('.')
                    });
                    GetNextToken();
                }
                src.Add(new ProgramElement() { token = Token.EOF, value = lex.Value, identifier = "" });
            }
            catch
            {
                return false;
            }

            program.elements = src.ToArray();

            int numOfElements = program.elements.Count();
            if (numOfElements > 0) for (var inx = 1; inx <= numOfElements; inx++) // (!) inx starts from 1 here not 0
                {
                    if ((inx + 1) == numOfElements) break;
                    if (program.elements[inx - 1].token == Token.Let &&
                        program.elements[inx].token == Token.Identifier &&
                        program.elements[inx + 1].token == Token.Equal) // (<) locate the "LET identifier =" statement
                    {
                        if (program.elements[inx].isDoted) continue; // (<) do not list doted variables. normally will not found in LET statement

                        var name = program.elements[inx].identifier;
                        if (program.variables.ContainsKey(name)) continue; // (<) if already in list goto next

                        program.variables.Add(name, ValueType.Real); // add the variable to the list. The type Real is the default type

                        // (v) now check the next elements and try to extract variable type
                        inx = inx + 2; if (inx == numOfElements) break; // (<) goto next element +1 is the "=", +2 is the next

                        if (program.elements[inx].token == Token.Value)
                        {
                            program.variables[name] = program.elements[inx].value.Type; // (<) we are sure about the type
                            continue;
                        }

                        // (v) now to identify the type go forward until find a value or a line change
                        inx++; // goto next element
                        bool exitLoop = false;
                        while (inx <= numOfElements)
                        {
                            switch (program.elements[inx].token)
                            {
                                case Token.Value:
                                    program.variables[name] = program.elements[inx].value.Type; // (<) we are sure about the type
                                    exitLoop = true;
                                    break;
                                case Token.EOF:
                                case Token.Semicolon:
                                case Token.NewLine:
                                    exitLoop = true;
                                    break;
                            }
                            if (exitLoop) break;
                            inx++;
                        }
                    }

                }
            return true;
        }

        /// <summary>
        /// Get the current state of the interpreter 
        /// </summary>
        /// <returns></returns>
        public InterpretationState GetState()
        {
            return new InterpretationState()
            {
                lastToken = this.lastToken,
                prevToken = this.prevToken,
                lineMarker = this.lineMarker,
                lexerState = this.lex.GetState()
            };
        }

        /// <summary>
        /// Set/reset the current state of the interpreter
        /// </summary>
        /// <param name="state"></param>
        public void SetState(InterpretationState state)
        {
            this.lastToken = state.lastToken;
            this.prevToken = state.prevToken;
            this.lineMarker = state.lineMarker;
            this.lex.SetState(state.lexerState);
        }


        #endregion (+) Program execution & control 

        #region (+) Other
        /// <summary>
        /// Dumps to the log information regarding the current parsing stage
        /// </summary>
        public void dumpInterpreter(bool interactive = false)
        {
            var oldState=GetState();
            while (true)
            {
                logger.info($"Line Maker.:  Line:{lineMarker.Line}, Column:{lineMarker.Column}, Pointer:{lineMarker.Pointer}");
                logger.info($"Token Maker:  Line:{lex.CurrentSourceMarker.Line}, Column:{lex.CurrentSourceMarker.Column}, Pointer:{lex.CurrentSourceMarker.Pointer}");

                logger.info($"Token: Prev.: {prevToken}, Last:{lastToken}");
                logger.info($"Last Char.: {lex.lastChar}");

                logger.info(lex.GetLine(lineMarker));
                logger.info(lex.GetLine(lex.CurrentSourceMarker));

                logger.info($"Variables: {vars.Count},   Functions: {funcs.Count}, Arrays: {arrays.Count}, Collections: {collections.Count}");
                logger.info($"Labels: {labels.Count},    Loops: {loops.Count},     Instr.Stack: {instructionStack.Count}, If.Counter: {ifCounter}");
                
                if (!interactive) break;

                Console.Write(">>>n=nextToken, r=restore. x=exit, d=dump >>>");
                var cmd = Console.ReadLine().ToUpper();
                bool exit = false;
                switch (cmd)
                {
                    case "N":
                        GetNextToken();
                        break;
                    case "R":
                        SetState(oldState);
                        break;
                    case "X":
                    case "Q":
                        exit = true;
                        break;
                    case "VAR":
                    case "V":
                        logger.info("Variables:");
                        foreach (var item in vars) logger.debug($"{item.Key}={item.Value}");
                        break;
                    case "ARR":
                    case "A":
                        logger.info("Arrays:");
                        foreach (var item in arrays) logger.debug($"{item.Key}={item.Value.ToString()}");
                        break;
                    case "COL":
                    case "C":
                        logger.info("Collections:");
                        foreach (var item in collections) logger.debug($"{item.Key}={item.Value.ToString()}");
                        break;
                    case "FUC":
                    case "F":
                        logger.info("Functions:");
                        foreach (var item in funcs) logger.debug($"{item.Key}={item.Value.ToString()}");
                        break;
                }
                if (exit) break;
                Console.WriteLine();
            }
            SetState(oldState);
        }
        #endregion (+) Other

    }
}
