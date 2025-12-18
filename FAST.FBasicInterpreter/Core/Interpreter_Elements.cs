namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC ELEMENTS EXCEPT STATMENTS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {

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

        public Value ValueOrVariable(bool doMatch=false)
        {
            if (doMatch) this.MatchAny(Token.Identifier, Token.Value);
            switch (lastToken)
            {
                case Token.Value:
                    return this.lex.Value;
                case Token.Identifier:
                    return GetValue(this.lex.Identifier); 
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
        public Value GetIdentifierOrCF(bool permitIdentifier = true,
                                        bool permitCollection = true,
                                        bool permitFunc = true)
        {
            var parser = new IdentifierNotationParser(lex.Identifier, this);
            if (parser.IsArray)
            {
                return parser.IsDataElementNumeric?
                    this.GetArray(parser.DataContainerName)[parser.ArrayIndex-1, parser.DataElementAsNumber-1] // 0-base reference
                    :
                    this.GetArray(parser.DataContainerName)[parser.ArrayIndex, parser.DataElement] // 1-base reference
                    ;
            }
            else if (parser.IsCollection)
            {
                if (!permitCollection) return Error(Errors.E124_NotPermitted("Collection expressions")).value;
                return this.GetCollectionOrArray(parser.DataContainerName).getValue(parser.DataElement);
            }
            else if (parser.IsFunction)
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
            else
            {
                return this.GetVar(lex.Identifier);
            }



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
                    return Error($"Undeclared name {lex.Identifier} [E111].").value;
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
                else if (arrays.ContainsKey(firstPart))
                {
                    if (!permitCollection) return Error(Errors.E124_NotPermitted("Collection and array expressions")).value;
                    throw new NotImplementedException();
                }
                else
                {
                    return Error(Errors.E112_UndeclaredEntity("variable", lex.Identifier)).value;
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

