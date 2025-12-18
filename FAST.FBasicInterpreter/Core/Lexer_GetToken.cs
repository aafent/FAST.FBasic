namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Lexicographic analysis - Parser 
    /// GETTOKEN Part
    /// </summary>
    public partial class Lexer : IInterpreterLexer
    {
        /// <summary>
        /// Used to return one Token, the next token from the current marker.
        /// </summary>
        /// <returns>The token</returns>
        public Token GetToken()
        {
            // skip white chars
            while (lastChar == ' ' || lastChar == '\t' || lastChar == '\r')
                GetChar();

            TokenMarker = sourceMarker;

            if (char.IsLetter(lastChar))
            {
                var tmpIdentifier = lastChar.ToString();
                while (char.IsLetterOrDigit(GetChar()))
                    tmpIdentifier += lastChar;

                this.Identifier = tmpIdentifier; // for performance field vs property
                var identifierUpper = this.Identifier.ToUpper();
                switch (identifierUpper)
                {
                    // (v) high priority 
                    case "LET": return Token.Let;

                    // (v) program flow
                    case "IF": return Token.If;
                    case "ENDIF": return Token.EndIf;
                    case "THEN": return Token.Then;
                    case "ELSE": return Token.Else;
                    case "FOR": return Token.For;
                    case "TO": return Token.To;
                    case "NEXT": return Token.Next;
                    case "GOTO": return Token.Goto;
                    case "GOSUB": return Token.Gosub;
                    case "RETURN": return Token.Return;
                    case "CALL": return Token.Call;
                    case "CHAIN": return Token.Chain;
                    case "EVAL": return Token.Eval;
                    case "FOREACH": return Token.ForEach;
                    case "ENDFOREACH": return Token.EndForEach;

                    // (v) statements
                    case "PRINT": return Token.Print;
                    case "INPUT": return Token.Input;
                    case "OR": return Token.Or;
                    case "AND": return Token.And;
                    case "NOT": return Token.Not;
                    case "RESULT": return Token.Result;
                    case "RINPUT": return Token.RInput;

                    // (v) Collections
                    case "SDATA": return Token.SData;

                    // (v) debugging statements
                    case "ASSERT": return Token.Assert;
                    case "DUMP": return Token.Dump;

                    // (v) low priority
                    case "END": return Token.End;
                    case "HALT": return Token.End;
                    case "WAIT": return Token.Wait;

                    // (v) Others
                    case "REM":
                        while (lastChar != '\n') GetChar();
                        GetChar();
                        return GetToken();
                    default:
                        // (v) check for add on statement
                        if (statements.Any(s => s == identifierUpper))
                        {
                            this.AddOn = identifierUpper;
                            return Token.AddOn;
                        }

                        // (v) return identifier
                        return Token.Identifier;
                }
            }

            if (char.IsDigit(lastChar))
            {
                string num = "";
                do { num += lastChar; } while (char.IsDigit(GetChar()) || lastChar == '.');

                double real;
                if (!double.TryParse(num, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out real))
                    FatalError("ERROR while parsing number");
                Value = new Value(real);
                return Token.Value;
            }

            string str;
            Token tok = Token.Unknown;
            switch (lastChar)
            {
                case '\n': tok = Token.NewLine; break;
                case ':': tok = Token.Colon; break;
                case ';': tok = Token.Semicolon; break;
                case ',': tok = Token.Comma; break;
                case '=': tok = Token.Equal; break;

                // (v) Math symbols and operators
                case '+': tok = Token.Plus; break;
                case '-': tok = Token.Minus; break;
                case '/': tok = Token.Slash; break;
                case '*': tok = Token.Asterisk; break;
                case '^': tok = Token.Caret; break;
                case '(': tok = Token.LParen; break;
                case ')': tok = Token.RParen; break;

                // (v) bracket identifiers [A.B], [table.column] etc
                case '[':
                    Identifier = string.Empty;  // (<) trying to parse [table.column] or [class.property] etc
                    while (GetChar() != ']')
                    {
                        switch (lastChar)
                        {
                            case ']': break; // well done
                            case '[':
                                FatalError("Found opening bracket but expected closing [X003]");
                                break;
                            case '\n':
                                FatalError("Found end of line but expected closing bracket [X004]");
                                break;
                        }

                        Identifier += lastChar;
                    }
                    var p1 = Identifier.IndexOf('.');
                    if (p1 < 0) FatalError("Identifier inside brackets must contains a dot character [X005]");
                    if (p1 == 0 || p1 == Identifier.Length) FatalError("Bracket Identifier cannot starts or ends with dot character [X006]");
                    GetChar(); // move 1 character to bypass the ]
                    return Token.Identifier;

                // (v) inline comment
                case '\'':
                    // skip comment until new line
                    while (lastChar != '\n') GetChar();
                    //GetChar();
                    return Token.NewLine;

                // (v) Comparison operators <,>,<>,<=,>=
                case '<':
                    GetChar();
                    if (lastChar == '>') tok = Token.NotEqual;
                    else if (lastChar == '=') tok = Token.LessEqual;
                    else return Token.Less;
                    break;
                case '>':
                    GetChar();
                    if (lastChar == '=') tok = Token.MoreEqual;
                    else return Token.More;
                    break;

                // (v) The quote (string) 
                case '"':
                    str = "";
                    while (GetChar() != '"')
                    {
                        if (lastChar == '\\')
                        {
                            // parse \n, \t, \\, \"
                            switch (char.ToLower(GetChar()))
                            {
                                case 'n': str += '\n'; break;
                                case 't': str += '\t'; break;
                                case '\\': str += '\\'; break;
                                case '"': str += '"'; break;
                            }
                        }
                        else
                        {
                            str += lastChar;
                        }
                    }
                    Value = new Value(str);
                    tok = Token.Value;
                    break;

                // (v) EOF
                case (char)0:
                    return Token.EOF;
            }

            GetChar();
            return tok;
        }




    }
}
