namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Lexicographic analysis - Parser 
    /// </summary>
    public class Lexer : IInterpreterLexer
    {
        //private readonly string source;
        private string source;
        private Marker sourceMarker; // current position in source string
        public char lastChar { get; private set; }

        private List<string> statements = new();
        public delegate ErrorReturnClass ErrorHandler(string text);
        private ErrorHandler Error = null;

        /// <summary>
        /// Current source instruction marker. 
        /// </summary>
        public Marker CurrentSourceMarker { get => sourceMarker; }

        public Marker TokenMarker { get; set; }

        public string Identifier { get; set; } // Last encountered identifier
        public Value Value { get; set; } // Last number or string
        public string AddOn { get; set; } // Last AddOn Statement found

        public Lexer(string input, ErrorHandler error)
        {
            source = input;
            this.Error = error;

            sourceMarker = new Marker(0, 1, 1);
            lastChar = source[0];

        }

        /// <summary>
        /// Fatal error found, call the error handler if exists
        /// </summary>
        private void FatalError(string text)
        {
            if (Error == null)
            {
                throw new Exception(text);
            }
            else
            {
                Error(text);
                throw new Exception("FATAL ERROR FOUND [X002]");
            }
        }


        /// <summary>
        /// Restart the program's execution from the begin
        /// </summary>
        public void RestartProgram()
        {
            sourceMarker = new Marker(0, 1, 1);
            lastChar = source[0];
        }

        /// <summary>
        /// Set the add statements 
        /// </summary>
        /// <param name="names">Array with the name of the statements</param>
        public void SetAddonStatements(string[] names)
        {
            this.statements = names.Select(s => s.ToUpper()).ToList();
        }

        /// <summary>
        /// Set the instruction pointer to a specific marker
        /// </summary>
        /// <param name="marker"></param>
        public void GoTo(Marker marker)
        {
            this.sourceMarker = marker;
            // (!) andreas: maybe here we should set the lastChar using the marker's pointer 
        }

        /// <summary>
        /// Get the current line where the marker is pointing
        /// This used only for debug purposes
        /// </summary>
        /// <param name="marker">The marker</param>
        /// <returns>The source line</returns>
        public string GetLine(Marker marker)
        {
            Marker oldMarker = sourceMarker;
            marker.Pointer--;
            GoTo(marker);

            string line = "";
            do
            {
                line += GetChar();
            } while (lastChar != '\n' && lastChar != (char)0);

            line.Remove(line.Length - 1);

            GoTo(oldMarker);

            return line;
        }


        private char GetChar()
        {
            sourceMarker.Column++;
            sourceMarker.Pointer++;

            if (sourceMarker.Pointer >= source.Length)
                return lastChar = (char)0;

            if ((lastChar = source[sourceMarker.Pointer]) == '\n') // advance a line (new line)
            {
                sourceMarker.Column = 1;
                sourceMarker.Line++;
            }
            return lastChar;
        }

        /// <summary>
        /// Set the value for the last character
        /// </summary>
        /// <param name="value">The value</param>
        public void SetLastChar(char value)
        {
            lastChar = value;
        }

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
                Identifier = lastChar.ToString();
                while (char.IsLetterOrDigit(GetChar()))
                    Identifier += lastChar;

                var identifierUpper = Identifier.ToUpper();
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

        /// <summary>
        /// Returns the source program
        /// </summary>
        /// <returns>string, the source program</returns>
        public string GetSource()
        {
            return this.source;
        }

        public void SetSource(string sourceProgram)
        {
            this.source = sourceProgram;
            sourceMarker = new Marker(0, 1, 1);
            lastChar = source[0];
        }

        /// <summary>
        /// Get a line (by number) from the source
        /// Do not use this method except for error presentation purposes 
        /// as it is very slow in performance.
        /// </summary>
        /// <param name="lineNumber">The requested line number</param>
        /// <returns>The extracted line number</returns>
        public string GetLine(int lineNumber)
        {
            if (string.IsNullOrEmpty(this.source))
            {
                return null;
            }

            // Split the string into an array of lines
            // StringSplitOptions.None ensures that empty lines are included in the result.
            // StringSplitOptions.TrimEntries removes any leading or trailing whitespace.
            string[] lines = this.source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Check if the array has at least the requested number of lines
            if (lines.Length >= lineNumber)
            {
                // The 5th line is at index 4 (since arrays are 0-indexed)
                // Use ElementAtOrDefault to safely get the element, returning null if the index is out of bounds
                return lines.ElementAtOrDefault(lineNumber - 1); //?.Trim()
            }

            return null;
        }


        public LexerState GetState()
        {
            return new LexerState()
            {
                Source = this.source,
                SourceMarker = new Marker(this.sourceMarker),
                LastChar = this.lastChar,
                TokenMarker = this.TokenMarker,
                Identifier = this.Identifier,
                Value = this.Value,
                AddOn = this.AddOn
            };
        }

        public void SetState(LexerState state)
        {
            this.source = state.Source;
            this.sourceMarker = new Marker(state.SourceMarker);
            this.lastChar = state.LastChar;
            this.TokenMarker = state.TokenMarker;
            this.Identifier = state.Identifier;
            this.Value = state.Value;
            this.AddOn = state.AddOn;
        }


    }
}
