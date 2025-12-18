namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC ELEMENTS EXCEPT STATMENTS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {

        /// <summary>
        /// Statement method
        /// </summary>
        private void Statement()
        {
            BuildIn_Statement(out Token keyword, out bool keywordFound);
            if (!keywordFound)
            {
                keywordFound = true;
                switch (keyword)
                {
                    case Token.AddOn:
                        // (v) Give priority to native sync method, otherwise invoke the async as sync.
                        if (this.statementsSync.ContainsKey(lex.AddOn))
                        {
                            // (v) native Sync method,just invoke it. 
                            this.statementsSync[lex.AddOn](this);
                        } 
                        else if (this.statementsAsync.ContainsKey(lex.AddOn))
                        {
                            // (v) invoke with the awaiter. 
                            this.statementsAsync[lex.AddOn](this).GetAwaiter().GetResult();
                        }
                        break;

                    case Token.Wait:
                        Thread.Sleep(waitStatement());
                        break;

                    default:
                        keywordFound = false;
                        break;
                }
                BuildIn_NotAStatement(keyword, keywordFound, out bool doStatement);
                if (doStatement)
                {
                    Statement();
                }
            }
        }

        private void Line()
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

    }
}

