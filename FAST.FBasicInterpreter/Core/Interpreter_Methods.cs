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
        /// Get underlying variables and values 
        /// </summary>
        /// <returns>Dictionary, the key is the variable name</returns>
        public Dictionary<string,Value> GetVariables()
        {
            return this.vars;
        }

        // (>) GetVar is at Program execution & control region. 

        #endregion (+) Add and more for variables

        #region (+) Add elements

        /// <summary>
        /// Add a new function
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="function">A reference to the function implementation</param>
        public void AddFunction(string name, BasicFunction function)
        {
            if (!funcs.ContainsKey(name)) funcs.Add(name, function);
            else funcs[name] = function;
        }

        /// <summary>
        /// Add a new statement
        /// </summary>
        /// <param name="name">The name of the statement</param>
        /// <param name="statment">A reference to the statement method implementation</param>
        public void AddStatement(string name, BasicStatement statement)
        {
            name=name.ToUpper(); // crucial to be upper
            if (!statements.ContainsKey(name)) statements.Add(name, statement);
            else statements[name] = statement;

        }

        /// <summary>
        /// Add data adapter. 
        /// Adapter can added only once
        /// </summary>
        /// <param name="adapter">The adapter (IfbasicDataAdapter) </param>
        public void AddDataAdapter(IfbasicDataAdapter adapter)
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
        public T GetDataAdapter<T>(string name) where T: IfbasicDataAdapter
        {
            return (T)dataAdapters[name];
        }

        /// <summary>
        /// Add or Set collection to the interpreter
        /// </summary>
        /// <param name="name">The collection name</param>
        /// <param name="collection">The collection handler</param>
        public void AddCollection(string name, IBAasicCollection collection)
        {
            if (!dataAdapters.ContainsKey(name))
            {    
                collections.Add(name, collection);
            }
            else
            {
                collections[name]=collection;
            }
        }

        /// <summary>
        /// Request For Object
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="group">The group</param>
        /// <param name="name">The name</param>
        /// <returns>The requested object or null</returns>
        public object requestForObject(string context, string group, string name, bool errorIfNull = true)
        {
            if (this.requestForObjectHandler == null) 
                Error(context, $"The Request For Object Handler is not installed ({context},{group},{name}) [E100]");
            var returnObject = this.requestForObjectHandler(context, group, name);
            if (errorIfNull & (returnObject == null)) 
                Error(context, $"Cannot get object for: ({context},{group},{name}) [E101]");
            return returnObject;
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
            string error =$"{source}: {text}";
            return Error(error);
        }
        private ErrorReturnClass Error(string text)
        {
            if (lineMarker.Line > 0)
            {
                string line=lex.GetLine(lineMarker.Line);
                if (!string.IsNullOrEmpty(line))
                {
                    if (lineMarker.Column >= 0 && lineMarker.Column <= line.Length)
                    {
                        line = line.Insert(lineMarker.Column, "<--E  ");
                    }
                }
                throw new fBasicException(text, lineMarker, line);
            } else
            {
                throw new fBasicException(text, lineMarker);
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

        private Token interpreter_GetNextToken()
        {
            prevToken = lastToken;
            lastToken = lex.GetToken();


            if (lastToken == Token.EOF && prevToken == Token.EOF)
                Error($"Unexpected end of program [E103]");

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
                throw new fBasicException($"Variable with name {name} does not exist. [X101]", lineMarker.Line);
            return vars[name];
        }

        /// <summary>
        /// Execute the program
        /// </summary>
        public void Exec()
        {
            lex.SetAddonStatements(statements.Keys.ToArray());
            GetNextToken=interpreter_GetNextToken;
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
            this.instructionStack=new();
            this.ifCounter=0; 
            // (!) check if we need more "resets" here. Check the commonConstructor()
        }

        /// <summary>
        /// Create a program container
        /// </summary>
        /// <returns>The program container</returns>
        public bool tryParseSourceCode(out ProgramContainer program)
        {
            program= new();
            List<ProgramElement> src = new();
            program.variables=new();
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
                    lineMarker = lex.TokenMarker; // save current line marker
                    src.Add(new ProgramElement() { token = lastToken, 
                                                   value = lex.Value, 
                                                   identifier = lex.Identifier, 
                                                   isDoted=lex.Identifier.Contains('.') 
                                                  });
                    GetNextToken();
                }
                src.Add(new ProgramElement() { token = Token.EOF, value = lex.Value, identifier = "" });
            } 
            catch
            {
                return false;
            }

            program.elements= src.ToArray();

            int numOfElements = program.elements.Count();
            if (numOfElements > 0) for (var inx=1; inx<=numOfElements; inx++) // (!) inx starts from 1 here not 0
            {
                if ( (inx+1) == numOfElements ) break; 
                if (program.elements[inx - 1].token == Token.Let  &&
                    program.elements[inx ].token == Token.Identifier &&
                    program.elements[inx + 1].token == Token.Equal ) // (<) locate the "LET identifier =" statement
                {
                        if (program.elements[inx].isDoted) continue; // (<) do not list doted variables. normally will not found in LET statement

                        var name= program.elements[inx].identifier;
                        if (program.variables.ContainsKey(name)) continue; // (<) if already in list goto next

                        program.variables.Add(name, ValueType.Real); // add the variable to the list. The type Real is the default type

                        // (v) now check the next elements and try to extract variable type
                        inx = inx+2; if (inx == numOfElements) break; // (<) goto next element +1 is the "=", +2 is the next

                        if (program.elements[inx].token == Token.Value)
                        {
                            program.variables[name]=program.elements[inx].value.Type; // (<) we are sure about the type
                            continue;
                        }
                            
                        // (v) now to identify the type go forward until find a value or a line change
                        inx++; // goto next element
                        bool exitLoop=false;
                        while ( inx <= numOfElements )
                        {
                            switch(program.elements[inx].token)
                            {
                                case Token.Value:
                                    program.variables[name] = program.elements[inx].value.Type; // (<) we are sure about the type
                                    exitLoop=true;
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

        #endregion (+) Program execution & control 

        #region (+) Other
        /// <summary>
        /// Dumps to the log information regarding the current parsing stage
        /// </summary>
        public void dumpInterpreter(bool interactive=false)
        {
            Marker oldLineMarker=new(lineMarker); // save the marker
            while(true)
            {
                log.info($"Line Maker.:  Line:{lineMarker.Line}, Column:{lineMarker.Column}, Pointer:{lineMarker.Pointer}");
                log.info($"Token Maker:  Line:{lex.TokenMarker.Line}, Column:{lex.TokenMarker.Column}, Pointer:{lex.TokenMarker.Pointer}");

                log.info($"Token: Prev.: {prevToken}, Last:{lastToken}");
                log.info($"Last Char.: {lex.lastChar}");

                log.info(lex.GetLine(lineMarker));
                log.info(lex.GetLine(lex.TokenMarker));

                if (!interactive) break;
 
                Console.Write(">>>n=nextToken, r=restore. x=exit, d=dump >>>");
                var cmd = Console.ReadLine().ToUpper();
                bool exit=false;
                switch (cmd)
                { 
                    case "N": 
                        GetNextToken();
                        break;
                    case "R":
                        lineMarker = new(oldLineMarker); // restore the marker
                        break;
                    case "X":
                    case "Q":
                        exit=true;
                        break;
                    case "D":
                        foreach (var item in vars)
                        {
                            log.debug($"{item.Key}={item.Value}");
                        }
                        break;
                }
                if (exit) break;
                Console.WriteLine();
            }
            lineMarker =new(oldLineMarker); // restore the marker
        }
        #endregion (+) Other

    }
}
