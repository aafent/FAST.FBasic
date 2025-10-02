namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC STATMENTS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        #region (+) methods to implement the FBASIC Flow statements 

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
            /*
            if (!labels.ContainsKey(name))
            {
                // if we didn't encounter required label yet, start to search for it
                while (true)
                {
                    if (GetNextToken() == Token.Colon && prevToken == Token.Identifier)
                    {
                        if (!labels.ContainsKey(lex.Identifier))
                            labels.Add(lex.Identifier, lex.TokenMarker);
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
            */
            lastToken = Token.NewLine;
        }
        void Return()
        {
            if (instructionStack.Count < 1)
                Error($"Found RETURN without corresponding GOSUB [E121]");
            lex.GoTo(instructionStack.Pop());

            //// (v) Advance to next command
            //while(false)
            //{
            //    var xx=prevToken;
            //    var yy=lastToken;
            //    GetNextToken(); 
            //    if (lastToken == Token.NewLine) break;
            //    if (lastToken == Token.EOF ) break;
            //    if (lastToken == Token.Colon ) break;
            //    continue;
            //}
            ////GetNextToken(); // bypass the GOSUB LABEL that was Pushed 


            SetLastTokenToNewLine();
        }
        void CallStatement()
        {
            if (callHandler == null) Error("There is no handler for the CALL statement [E109]");

            string name = Expr().ToString();
            var src = callHandler(name);
            Interpreter sub = new(false, src); // BuiltIns will passed with funcs and statements

            sub.printHandler = this.printHandler;
            sub.inputHandler = this.inputHandler;
            sub.callHandler = this.callHandler;
            sub.requestForObjectHandler = this.requestForObjectHandler;
            sub.funcs = this.funcs;
            sub.statements = this.statements;
            if (!this.IsVariable("RESULTVALUE")) this.SetVar("RESULTVALUE", Value.Zero);
            sub.SetVar("RESULTVALUE", this.GetVar("RESULTVALUE")!);
            var subResult = sub.ExecWithResult();
            if (subResult.hasError) Error($"CALL return's error: {subResult.errorText} [E118]");
            this.SetVar("RESULTVALUE", subResult.value);
            this.Result = subResult.value;
        }
        void Halt()
        {
            exit = true;
        }
        void Label()
        {
            string name = lex.Identifier;
            if (!labels.ContainsKey(name)) labels.Add(name, lex.CurrentSourceMarker); // labels.Add(name, lex.TokenMarker);

            GetNextToken();
            Match(Token.NewLine);
        }

        #endregion (+) methods to implement the FBASIC Flow statements 

        #region (+) methods to implement the FBASIC Loop statements 
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
        void Next()
        {
            // jump to beginning of the "for" loop
            Match(Token.Identifier);
            string var = lex.Identifier;
            vars[var] = vars[var].BinOp(new Value(1), Token.Plus);
            lex.GoTo(new Marker(loops[var].Pointer - 1, loops[var].Line, loops[var].Column - 1));
            lastToken = Token.NewLine;
        }

        void ForEachStatement()
        {
            // Syntax: FOREACH collectionName
            //  Loop (FOREACH..ENDFOREACH) the items of a collection

            this.Match(Token.Identifier);
            string collectionName = this.lex.Identifier;
            if (!this.collections.ContainsKey(collectionName)) Error("FOREACH", "Collection: {collectionName} not found. [E110]");

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
            var collection = this.collections[collectionName];
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
        void EndForEachStatement()
        {
            // jump to beginning of the "forEach" loop
            Match(Token.Identifier);
            string collectionName = lex.Identifier;
            string endForEachLoop = collectionName + "$ENDFOREACH";

            var collection = this.collections[collectionName];
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
        void If()
        {
            // check if argument is equal to 0
            bool result = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            Match(Token.Then);
            GetNextToken();

            if (result)
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
        void ResultStatement()
        {
            this.Result = Expr();
            SetVar("RESULTVALUE", this.Result);
        }

        void Print()
        {
            if (printHandler == null)
            {
                Expr(); // just to move the next instruction
            }
            else
            {
                printHandler?.Invoke(Expr().ToString());
            }

        }

        void Input()
        {
            while (true)
            {
                Match(Token.Identifier);

                if (!vars.ContainsKey(lex.Identifier)) vars.Add(lex.Identifier, new Value());

                string input = inputHandler?.Invoke();
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

        void Let()
        {
            if (lastToken != Token.Equal)
            {
                Match(Token.Identifier);
                GetNextToken();
                Match(Token.Equal);
            }

            string id = lex.Identifier;

            GetNextToken();

            SetVar(id, Expr());
        }

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

            var value=RequestForObject(context: "IN", group: group, name: name);

            SetVar(vname, new Value(value.ToString()) );

        }

    


        #endregion (+) methods to implement the FBASIC In-Out, Data & Values  statements 

        #region (+) methods to implement the FBASIC Other statements 

        void AddOnStatement()
        {
            this.statements[lex.AddOn](this);
        }

        void Assert()
        {
            bool result = (Expr().BinOp(new Value(0), Token.Equal).Real == 1);

            if (result)
            {
                Error("Assertion fault [E108]"); // if out assert evaluate to false, throw error with source code line
            }
        }

        void DumpStatement()
        {
            dumpInterpreter();
            string text = Expr().ToString();
            log.debug($"{text} at line:{lineMarker.Line}");
            foreach (var item in vars)
            {
                log.debug($"{item.Key}={item.Value}");
            }

        }

        #endregion (+) methods to implement the FBASIC Other  statements 
    }
}

