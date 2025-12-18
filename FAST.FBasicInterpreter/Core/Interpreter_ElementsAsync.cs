namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC ELEMENTS EXCEPT STATMENTS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        /// <summary>
        /// Async Statement method
        /// </summary>
        /// <returns></returns>
        private async Task StatementAsync()
        {
            BuildIn_Statement(out Token keyword, out bool keywordFound);
            if (!keywordFound)
            {
                keywordFound = true;
                switch (keyword)
                {
                    case Token.AddOn:
                        // (v) if give priority to async method if exists, otherwise run the sync.
                        if (this.statementsAsync.ContainsKey(lex.AddOn))
                        {
                            // (v) Invoke it a part of a Async chain of methods.
                            await this.statementsAsync[lex.AddOn](this);
                        } 
                        else if (this.statementsSync.ContainsKey(lex.AddOn))
                        {
                            // (v) invoke it as a Sync method
                            this.statementsSync[lex.AddOn](this);
                        }
                        break;
                    case Token.Wait:
                        await Task.Delay(waitStatement() );
                        break;

                    default:
                        keywordFound = false;
                        break;
                }
            }
            BuildIn_NotAStatement(keyword, keywordFound, out bool doStatement);
            if (doStatement)
            {
                await StatementAsync();
            }
        }

        private async Task LineAsync()
        {
            // skip empty new lines
            while (lastToken == Token.NewLine) GetNextToken();

            if (lastToken == Token.EOF)
            {
                exit = true;
                return;
            }

            lineMarker = lex.TokenMarker; // save current line marker
            await StatementAsync(); // evaluate statement

            if (lastToken != Token.NewLine && lastToken != Token.EOF)
            { 
                Error($"Expecting new line, got {lastToken.ToString()} [E104]");
            }
        }


    }
}
