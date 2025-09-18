namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC ELEMENTS EXCEPT STATMENTS
    /// </summary>
    public partial class Interpreter : IfbasicError
    {
        void Statement()
        {
            Token keyword = lastToken;
            GetNextToken();
            switch (keyword)
            {
                // (v) statement tokens 
                case Token.Print: Print(); break;
                case Token.Input: Input(); break;
                case Token.Goto: Goto(); break;
                case Token.Gosub: Gosub(); break;
                case Token.Return: Return(); break;
                case Token.If: If(); break;
                case Token.Else: Else(); break;
                case Token.EndIf: break;
                case Token.For: For(); break;
                case Token.Next: Next(); break;
                case Token.Let: Let(); break;
                case Token.End: Halt(); break;   // synonyms: END, HALT
                case Token.Assert: Assert(); break;
                case Token.AddOn: AddOnStatement(); break;
                case Token.Result: ResultStatement(); break;
                case Token.Dump: DumpStatement(); break;
                case Token.Call: CallStatement(); break;
                case Token.ForEach: ForEachStatement(); break;
                case Token.EndForEach: EndForEachStatement(); break;
                case Token.SData: SDataStatement(); break;    
                case Token.RInput: RInputStatement(); break;

                // (v) non-statement tokens 
                case Token.Identifier:
                    if (lastToken == Token.Equal) Let();
                    else if (lastToken == Token.Colon) Label();
                    else goto default;
                    break;
                case Token.EOF:
                    exit = true;
                    break;
                default:
                    string msg2= "";
                    switch (keyword)
                    {
                        case Token.Identifier:
                            msg2=lex.Identifier;
                            break;
                        case Token.Value:
                            msg2=lex.Value.ToString();
                            break;
                        default:
                            msg2="";
                            break;
                    }
                    Error(Errors.E106_ExpectingKeyword(keyword.ToString(), msg2));
                    break;
            }
            if (lastToken == Token.Colon)
            {
                // we can execute more statements in single line if we use ";"
                GetNextToken();
                Statement();
            }
        }

        void Line()
        {
            // skip empty new lines
            while (lastToken == Token.NewLine) GetNextToken();

            if (lastToken == Token.EOF)
            {
                exit = true;
                return;
            }

            lineMarker = lex.TokenMarker; // save current line marker
            Statement(); // evaluate statement

            if (lastToken != Token.NewLine && lastToken != Token.EOF)
            { 
                Error($"Expecting new line, got {lastToken.ToString()} [E104]");
            }
        }

        public Value Expr(int min = 0)
        {
            // originally we were using shunting-yard algorithm, but now we parse it recursively 
            Dictionary<Token, int> precedents = new Dictionary<Token, int>()
            {
                { Token.Or, 0 }, { Token.And, 0 },
                { Token.Equal, 1 }, { Token.NotEqual, 1 },
                { Token.Less, 1 }, { Token.More, 1 },
                { Token.LessEqual, 1 },  { Token.MoreEqual, 1 },
                { Token.Plus, 2 }, { Token.Minus, 2 },
                { Token.Asterisk, 3 }, {Token.Slash, 3 },
                { Token.Caret, 4 }
            };

            Value lhs = Primary();

            while (true)
            {
                if (lastToken < Token.Plus || lastToken > Token.And || precedents[lastToken] < min)
                    break;

                Token op = lastToken;
                int prec = precedents[lastToken]; // Operator Precedence
                int assoc = 0; // 0 left, 1 right; Operator associativity
                int nextmin = assoc == 0 ? prec : prec + 1;
                GetNextToken();
                Value rhs = Expr(nextmin);
                lhs = lhs.BinOp(rhs, op);
            }

            return lhs;
        }
        public Value ValueOrIdentifier()
        {
            switch(lastToken)
            {
                case Token.Value:
                    return this.lex.Value;
                case Token.Identifier:
                    return Value.Error; // TODO
                default:
                    Error($"Expecting Value or Identifier, got {lastToken.ToString()} [E123]");
                    return Value.Error;
            }
        }

        /// <summary>
        /// Get Identifier or Value or Collection or Function
        /// </summary>
        /// <param name="permitIdentifier">true if permit identifiers</param>
        /// <param name="permitCollection">true if permit collection items</param>
        /// <param name="permitFunc">true if permit functions</param>
        /// <returns>Value</returns>
        public Value GetIdentifierOrCF( bool permitIdentifier=true, 
                                        bool permitCollection=true,
                                        bool permitFunc=true )
        {
            string firstPart = null;
            string otherPart = null;
            var dotPosition = lex.Identifier.IndexOf('.');
            if (dotPosition >= 0) // (v) collections dotted identifier [collection.item]
            {
                firstPart = lex.Identifier.Substring(0, dotPosition);
                otherPart = lex.Identifier.Substring(dotPosition + 1);

                if (collections.ContainsKey(firstPart))
                {
                    if (!permitCollection) return Error(Errors.E124_NotPermitted("Collection expressions")).value;
                    return collections[firstPart].getValue(otherPart);
                }
                else
                {
                    return Error($"Undeclared name {lex.Identifier} [E111]").value;
                }
            }
            else
            {
                firstPart = lex.Identifier;
                otherPart = "ITEM";

                // syntax: ident | ident '(' args ')'
                if (vars.ContainsKey(lex.Identifier)) // just ident
                {
                    if (!permitIdentifier) return Error(Errors.E124_NotPermitted("Identifier(Variable)")).value;
                    return vars[lex.Identifier];
                }
                else if (funcs.ContainsKey(lex.Identifier))
                {
                    if (!permitFunc) return Error(Errors.E124_NotPermitted("Function")).value;
                    string name = lex.Identifier;
                    List<Value> args = new List<Value>();
                    GetNextToken();
                    Match(Token.LParen);

                start:
                    if (GetNextToken() != Token.RParen)
                    {
                        args.Add(Expr());
                        if (lastToken == Token.Comma)
                            goto start;
                    }
                    return funcs[name](this, args);
                }
                else if (collections.ContainsKey(firstPart))
                {
                    if (!permitCollection) return Error(Errors.E124_NotPermitted("Collection expressions")).value;
                    return collections[firstPart].getValue(otherPart);
                }
                else
                {
                    return Error(Errors.E112_UndeclaredVariable(lex.Identifier)).value;
                }
            }

            // (!>) a never-reach point of code, that's for no return statement here
        }

        Value Primary()
        {
            Value prim = Value.Zero;

            if (lastToken == Token.Value)
            {
                // number | string
                prim = lex.Value;
                GetNextToken();
            }
            else if (lastToken == Token.Identifier)
            {
                prim=GetIdentifierOrCF(); // all permitted
                /* method GetIdentifierOrVCF() replace the following code:
                string firstPart = null;
                string otherPart = null;
                var dotPosition = lex.Identifier.IndexOf('.');
                if (dotPosition >= 0)
                {
                    firstPart = lex.Identifier.Substring(0, dotPosition);
                    otherPart = lex.Identifier.Substring(dotPosition + 1);

                    if (collections.ContainsKey(firstPart))
                    {
                        prim = collections[firstPart].getValue(otherPart);
                    }
                    else
                    {
                        Error($"Undeclared name {lex.Identifier} [E111]");
                    }
                }
                else
                {
                    // ident | ident '(' args ')'
                    if (vars.ContainsKey(lex.Identifier))
                    {
                        prim = vars[lex.Identifier];
                    }
                    else if (funcs.ContainsKey(lex.Identifier))
                    {
                        string name = lex.Identifier;
                        List<Value> args = new List<Value>();
                        GetNextToken();
                        Match(Token.LParen);

                    start:
                        if (GetNextToken() != Token.RParen)
                        {
                            args.Add(Expr());
                            if (lastToken == Token.Comma)
                                goto start;
                        }

                        prim = funcs[name](this, args);
                    }
                    else if (collections.ContainsKey(firstPart))
                    {
                        prim = collections[firstPart].getValue(otherPart);
                    }
                    else
                    {
                        Error($"Undeclared variable {lex.Identifier} [E112]");
                    }
                }
                */

                // go to next token
                GetNextToken();
            }
            else if (lastToken == Token.LParen)
            {
                // '(' expr ')'
                GetNextToken();
                prim = Expr();
                Match(Token.RParen);
                GetNextToken();
            }
            else if (lastToken == Token.Plus || lastToken == Token.Minus || lastToken == Token.Not)
            {
                // unary operator
                // '-' | '+' primary
                Token op = lastToken;
                GetNextToken();
                prim = Primary().UnaryOp(op);
            }
            else
            {
                Error($"Unexpected token: {lastToken} in primary! [E113]");
            }

            return prim;
        }
    }
}

