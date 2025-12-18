namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Lexicographic analysis - Parser 
    /// </summary>
    public partial class Lexer : IInterpreterLexer
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input"></param>
        /// <param name="error"></param>
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
        /// Get Line from source
        /// </summary>
        /// <returns>a line</returns>
        public bool GetLine(out string line)
        {
            line="";
            if (lastChar==0)
            {
                return false;
            }
            if (lastChar == '\r') GetChar();
            while (lastChar != '\r')
            {
                line +=lastChar;
                GetChar();
                if (lastChar == 0)
                {
                    return false;
                }
            }
            TokenMarker = sourceMarker;
            return true;
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
