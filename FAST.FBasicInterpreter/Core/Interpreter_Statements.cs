using FAST.FBasicInterpreter.DataProviders;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC STATMENTS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) methods to implement the FBASIC Flow statements 

        /// <summary>
        /// GOTO
        /// </summary>
        void Goto()
        {
            Match(Token.Identifier);
            string name = lex.Identifier;

            if (!labels.ContainsKey(name))
            {
                // if we didn't encounter required label yet, start to search for it
                while (true)
                {
                    if (GetNextToken() == Token.Colon && prevToken == Token.Identifier)
                    {
                        if (!labels.ContainsKey(lex.Identifier))
                            labels.Add(lex.Identifier, lex.CurrentSourceMarker);  //labels.Add(lex.Identifier, lex.TokenMarker);
                        if (lex.Identifier == name)
                            break;
                    }
                    if (lastToken == Token.EOF)
                    {
                        Error($"Cannot find label named {name} [E107]");
                    }
                }
            }
            lex.GoTo(labels[name]);
            lastToken = Token.NewLine;
        }

        /// <summary>
        /// GOSUB (GOSUB...RETURN)
        /// </summary>
        void Gosub()
        {
            Match(Token.Identifier);
            string name = lex.Identifier;

            PushCurrentInstructionMarker();
            if (!SetFlowToLabelIfExists(name))
            {
                Error(Errors.E107_LabelNotFound(name));
                return;
            }
            lastToken = Token.NewLine;
        }

        /// <summary>
        /// RETURN (GOSUB...RETURN)
        /// </summary>
        void Return()
        {
            if (instructionStack.Count < 1)
                Error($"Found RETURN without corresponding GOSUB [E121]");
            lex.GoTo(instructionStack.Pop());

            SetLastTokenToNewLine();
        }


        /// <summary>
        /// CALL
        /// </summary>
        void CallStatement()
        {
            if (FileHandler== null)
            {
                Error(Errors.E109_NoFileHandlerInstalled("Cannot load program file."));
                return;
            }

            string name = Expr().ToString();

            var file = new FBasicSourceProgramFile(name);
            var src=FileHandler(file).GetSourceProgram();

            Interpreter sub = new(false, src); // BuiltIns will passed with funcs and statements

            sub.PrintHandler = this.PrintHandler;
            sub.InputHandler = this.InputHandler;
            sub.callHandler = this.callHandler;
            sub.FileHandler = this.FileHandler;
            sub.RequestForObjectHandler = this.RequestForObjectHandler;
            sub.funcs = this.funcs;
            sub.statements = this.statements;
            if (!this.IsVariable("RESULTVALUE")) this.SetVar("RESULTVALUE", Value.Zero);
            sub.SetVar("RESULTVALUE", this.GetVar("RESULTVALUE")!);
            var subResult = sub.ExecWithResult();
            if (subResult.hasError) Error($"CALL return's error: {subResult.errorText} [E118]");
            this.SetVar("RESULTVALUE", subResult.value);
            this.Result = subResult.value;
        }

        /// <summary>
        /// CHAIN
        /// </summary>
        void ChainStatement()
        {
            if (FileHandler == null)
            {
                Error(Errors.E109_NoFileHandlerInstalled("Cannot load program file."));
                return;
            }

            // (v) load the chain source program
            string programFilename = Expr().ToString();

            var file = new FBasicSourceProgramFile(programFilename);
            var srcCode = FileHandler(file).GetSourceProgram();

            // (v) save the current program state
            var oldState = this.GetState();

            //// (v) Set the new chain program and execute it
            lex.SetSource(srcCode);
            exit = false;
            GetNextToken();
            while (!exit) Line(); // do all lines
            exit = false;

            // (v) restore the old program state
            this.SetState(oldState);

            // (v) continue with the next instruction

            return;
        }

        /// <summary>
        /// EVAL variable_name 
        /// </summary>
        void EvalStatement()
        {
            // (v) load the chain source program
            string srcCode = Expr().ToString();

            // (v) save the current program state
            var oldState = this.GetState();

            //// (v) Set the new chain program and execute it
            lex.SetSource(srcCode);
            exit = false;
            GetNextToken();
            while (!exit) Line(); // do all lines
            exit = false;

            // (v) restore the old program state
            this.SetState(oldState);

            // (v) continue with the next instruction

            return;
        }



        /// <summary>
        /// HALT | END
        /// </summary>
        void Halt()
        {
            exit = true;
        }

        /// <summary>
        /// label:
        /// </summary>
        void Label()
        {
            string name = lex.Identifier;
            if (!labels.ContainsKey(name)) labels.Add(name, lex.CurrentSourceMarker); // labels.Add(name, lex.TokenMarker);

            GetNextToken();
            Match(Token.NewLine);
        }

        #endregion (+) methods to implement the FBASIC Flow statements 

        #region (+) methods to implement the FBASIC Loop statements 

        /// <summary>
        /// FOR (FOR...NEXT)    
        /// </summary>
        void For()
        {
            Match(Token.Identifier);
            string var = lex.Identifier;

            GetNextToken();
            Match(Token.Equal);

            GetNextToken();
            Value v = Expr();

            // save for loop marker
            if (loops.ContainsKey(var))
            {
                loops[var] = lineMarker;
            }
            else
            {
                SetVar(var, v);
                loops.Add(var, lineMarker);
            }

            Match(Token.To);

            GetNextToken();
            v = Expr();

            if (vars[var].BinOp(v, Token.More).Real == 1)
            {
                while (true)
                {
                    while (!(GetNextToken() == Token.Identifier && prevToken == Token.Next)) ;
                    if (lex.Identifier == var)
                    {
                        loops.Remove(var);
                        GetNextToken();
                        Match(Token.NewLine);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// NEXT (FOR...NEXT)
        /// </summary>
        void Next()
        {
            // jump to beginning of the "for" loop
            Match(Token.Identifier);
            string var = lex.Identifier;
            vars[var] = vars[var].BinOp(new Value(1), Token.Plus);
            lex.GoTo(new Marker(loops[var].Pointer - 1, loops[var].Line, loops[var].Column - 1));
            lastToken = Token.NewLine;
        }

        /// <summary>
        /// FOREACH (FOREACH...ENDFOREACH)
        /// </summary>
        void ForEachStatement()
        {
            // Syntax: FOREACH collectionName
            //  Loop (FOREACH..ENDFOREACH) the items of a collection

            this.Match(Token.Identifier);
            string collectionName = this.lex.Identifier;
            if (!this.IsCollectionOrArray(collectionName)) Error("FOREACH", "Collection/Array: {collectionName} not found. [E110]");

            this.GetNextToken();

            // (v) save for loop marker
            if (loops.ContainsKey(collectionName))
            {
                loops[collectionName] = lineMarker;
            }
            else
            {
                // SetVar(collectionName, new Value(collectionName) ); 
                loops.Add(collectionName, lineMarker);
            }

            // (v) FETCH
            var collection= this.GetCollectionOrArray(collectionName);
            collection.MoveNext();
            //collection.endOfData = !collection.MoveNext();

            if (collection.endOfData) // (<) by pass everything until the ENDFORECH
            {
                string endForEachLoop = collectionName + "$ENDFOREACH";
                if (!loops.ContainsKey(endForEachLoop))
                {
                    // (!v) The ENDFOREACH is not in the loops statement yet
                    bool endOfLoopFound = false;
                    while (lastToken != Token.EOF)
                    {
                        JumpToNextToken(Token.EndForEach);
                        this.Match(Token.EndForEach); // ensure that you are on the EndForEachStatement
                        GetNextToken(); // hope to get the loop name.
                                        // jump to beginning of the "forEach" loop
                        Match(Token.Identifier);
                        string argCollectionName = lex.Identifier;
                        if (argCollectionName == collectionName)
                        {
                            EndForEachStatement(); // execute the EndForEach statement 
                            endOfLoopFound = true;
                            break; // go out of the while loop.
                        }

                    } // while loop
                    if (!endOfLoopFound)
                    {
                        Error($"FOREACH {collectionName} without corresponding ENDFOREACH {collectionName} statement [E114].");
                    }
                    return; // end of the ForEach statement as the control has already moved to the EndForEach statement
                }

                // (v) set program flow to EndForEachStatement 
                lex.GoTo(loops[endForEachLoop].OnePointBack());
                SetLastTokenToNewLine();

                return;
            }


            return; // normal return
        }

        /// <summary>
        /// ENDFOREACH (FOREACH...ENDFOREACH)
        /// </summary>
        void EndForEachStatement()
        {
            // jump to beginning of the "forEach" loop
            Match(Token.Identifier);
            string collectionName = lex.Identifier;
            string endForEachLoop = collectionName + "$ENDFOREACH";

            var collection = this.GetCollectionOrArray(collectionName);
            if (collection.endOfData) // leave the EndForEach and go to the next command
            {
                loops.Remove(collectionName);
                loops.Remove(endForEachLoop);
                GetNextToken();
                Match(Token.NewLine);
                return;
            }

            // (v) save for loop marker
            if (loops.ContainsKey(endForEachLoop))
            {
                loops[endForEachLoop] = lineMarker;
            }
            else
            {
                loops.Add(endForEachLoop, lineMarker);
            }

            // GoTo FOREACH statement
            lex.GoTo(new Marker(loops[collectionName].Pointer - 1, loops[collectionName].Line, loops[collectionName].Column - 1));
            lastToken = Token.NewLine;
            
        }

        #endregion (+) methods to implement the FBASIC Loop statements 

        #region (+) methods to implement the FBASIC Decision statements 

        /// <summary>
        /// IF (IF...THEN...[ELSE]...ENDIF)
        /// </summary>
        /// 
        void If()
        {
            // check if argument is equal to 0
            bool isFALSE = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            Match(Token.Then);
            GetNextToken();
            bool isBlock = lastToken == Token.NewLine;

            // (v) Only for non-block IF..THEN..STATEMENT
            if ( !isBlock )
            {
                if (lastToken == Token.If )
                {
                    Error("Inline IF cannot contain IF. Use AND instead. [E131]");
                    return; // never executed. 
                }

                if (!isFALSE) 
                { 
                    Statement(); // execute the next statement
                    return; // exit
                }

                if (lastToken==Token.NewLine ) return;
                while (GetNextToken()!=Token.NewLine) { } // just to the next newline
                return; // exit
            }




            // (v) Only for block IF..THEN..ELSE..ENDIF

            if (isFALSE && isBlock) // Skip to matching ELSE or ENDIF, mind the nested IFs
            {
                // in case "if" evaluate to zero skip to matching else or endif
                int i = ifCounter;
                while (true)
                {
                    if (lastToken == Token.If)
                    {
                        i++;
                    }
                    else if (lastToken == Token.Else)
                    {
                        if (i == ifCounter)
                        {
                            GetNextToken();
                            return;
                        }
                    }
                    else if (lastToken == Token.EndIf)
                    {
                        if (i == ifCounter)
                        {
                            GetNextToken();
                            return;
                        }
                        i--;
                    }
                    GetNextToken();
                }
            }


        }

        /// <summary>
        /// ELSE (IF...THEN...[ELSE]...ENDIF)
        /// </summary>
        void Else()
        {
            // skip to matching endif
            int i = ifCounter;
            while (true)
            {
                if (lastToken == Token.If)
                {
                    i++;
                }
                else if (lastToken == Token.EndIf)
                {
                    if (i == ifCounter)
                    {
                        GetNextToken();
                        return;
                    }
                    i--;
                }
                GetNextToken();
            }
        }

        #endregion (+) methods to implement the FBASIC Decision statements 

        #region (+) methods to implement the FBASIC In-Out, Data & Values statements 
        /// <summary>
        /// RESULT
        /// </summary>
        void ResultStatement()
        {
            this.Result = Expr();
            SetVar("RESULTVALUE", this.Result);
        }

        /// <summary>
        /// PRINT
        /// </summary>
        void Print()
        {
            if (PrintHandler == null)
            {
                Expr(); // just to move the next instruction
                if (lastToken == Token.Semicolon) GetNextToken();
            }
            else
            {
                //printHandler?.Invoke(Expr().ToString());
                var message = Expr().ToString();
                if (lastToken == Token.Semicolon)
                {
                    // (>) do not append new line
                    PrintHandler?.Invoke(message);
                    GetNextToken();
                }
                else
                {
                    // (>) append new line
                    message += System.Environment.NewLine;
                    PrintHandler?.Invoke(message);
                }
            }

        }

        /// <summary>
        /// INPUT
        /// </summary>
        void Input()
        {
            while (true)
            {
                Match(Token.Identifier);

                if (!vars.ContainsKey(lex.Identifier)) vars.Add(lex.Identifier, new Value());

                string input = InputHandler?.Invoke();
                double d;
                // try to parse as double, if failed read value as string
                if (double.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                    vars[lex.Identifier] = new Value(d);
                else
                    vars[lex.Identifier] = new Value(input);

                GetNextToken();
                if (lastToken != Token.Comma) break;
                GetNextToken();
            }
        }

        /// <summary>
        /// LET
        /// </summary>
        void Let()
        {
            if (lastToken != Token.Equal)
            {
                Match(Token.Identifier);
                GetNextToken();
                Match(Token.Equal);
            }

            string id = lex.Identifier;
            if (id.IndexOf('.') >= 0 )
            {
                var parser=new IdentifierNotationParser(id,this);
                if (parser.IsArray)
                {
                    var array = GetArray(parser.DataContainerName);
                    GetNextToken();
                    array[parser.ArrayIndex, parser.DataElement]=Expr();
                }
                else
                {
                    Error(Errors.E134_SquareBracketNotation($"Cannot assign value to {id}."));
                    return;
                }
            }
            else
            {
                GetNextToken();
                SetVar(id, Expr());

            }

        }

        /// <summary>
        /// SDATA
        /// </summary>
        void SDataStatement()
        {
            // Syntax: SDATA collectionName item1,item2,item3,item...N
            //  
            staticDataCollection collection=null;
            this.Match(Token.Identifier);
            string collectionName = this.lex.Identifier;

            if ( this.collections.ContainsKey(collectionName) )
            {
                if (this.collections[collectionName] is staticDataCollection)
                {
                    collection = (staticDataCollection)this.collections[collectionName];
                }
                else
                {
                    Error(Errors.E117_CollectionIsNotSDATAType(collectionName));
                    return; // not necessary as Error is exception
                }
                
            }
            else
            {
                collection=new(this); 
                collection.Reset();
                this.AddCollection(collectionName, collection);
            }

            this.GetNextToken(); // move to the first item of the collection
            var value=lastToken==Token.Value?this.lex.Value:GetIdentifierOrCF(permitFunc:false); // the inline if? is for performance
            //this.Match(Token.Value); // match the first item of the collection
            collection.data.Add(value);

            this.GetNextToken(); // move to the comma (,) before the second collection's item 
            while (lastToken == Token.Comma)
            {
                this.GetNextToken(); // move to the next item of the collection
                value = lastToken == Token.Value ? this.lex.Value : GetIdentifierOrCF(permitFunc:false); // inline if? for performance

                //this.Match(Token.Value); // match the Value as the item of the collection
                collection.data.Add(value);

                this.GetNextToken(); // move to the comma (,) before the next collection's item 
            }

            return; // normal return
        }

        /// <summary>
        /// RINPUT
        /// </summary>
        void RInputStatement()
        {
            // Syntax: RINPUT group, name, identifier 
            //  
            this.Match(Token.Identifier);
            string group = this.lex.Identifier;

            GetNextToken();
            this.Match(Token.Comma);

            GetNextToken();
            this.Match(Token.Identifier);
            string name = this.lex.Identifier;

            GetNextToken();
            this.Match(Token.Comma);

            GetNextToken();
            this.Match(Token.Identifier);
            string vname = this.lex.Identifier;

            GetNextToken();
            var request = new FBasicRequestForObjectDescriptor(this,"IN");
            request.Group=group;
            request.Name=name;
            request.VariableName=vname;
            var value=RequestForObject(request);
            
            if (value is Value)
            {
                SetVar(vname,(Value)value);
                return;
            }
            if (value is string)
            {
                SetVar(vname, new Value(value.ToString()));
                return;
            }
            else if ((value is Double) | (value is int)) 
            {
                SetVar(vname, new Value((double)value));
                return;
            }
            else
            {
                SetVar(vname,new Value(value,value.GetType().ToString()));
                return;
            }

        }

        #endregion (+) methods to implement the FBASIC In-Out, Data & Values  statements 

        #region (+) methods to implement the FBASIC Other statements 

        /// <summary>
        /// any ADDON statement
        /// </summary>
        void AddOnStatement()
        {
            this.statements[lex.AddOn](this);
        }

        /// <summary>
        /// ASSERT
        /// </summary>
        void Assert()
        {
            bool result = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            if (result)
            {
                Error("Assertion fault [E108]"); // if out assert evaluate to false, throw error with source code line
            }
        }

        /// <summary>
        /// DUMP
        /// </summary>
        void DumpStatement()
        {
            dumpInterpreter();
            string text = Expr().ToString();
            logger.debug($"{text} at line:{lineMarker.Line}");
            foreach (var item in vars)
            {
                logger.debug($"{item.Key}={item.Value}");
            }

        }

        #endregion (+) methods to implement the FBASIC Other  statements 
    }
}

