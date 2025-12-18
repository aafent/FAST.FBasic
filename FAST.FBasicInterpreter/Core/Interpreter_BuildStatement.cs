namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// The interpreter factory of the FBASIC 
    /// Part: THE FBASIC STATEMENT METHODS
    /// </summary>
    public partial class Interpreter : IFBasicError
    {
        /*
         * Execution flow: A->B->C
         * [A]BuildIn_Statement -> [B]your code to manipulate keywords -> [C]BuildIn_NotAStatement
         * Rules:
         *      if A.tokenfound -> C
         *      From B do always -> C
         *      if C.doStatement -> call again (recursive) the method (eg Statement()) 
         */

        private void BuildIn_Statement(out Token keyword, out bool keywordFound)
        {
            keywordFound = true;
            keyword = lastToken;
            GetNextToken();
            if (keyword == Token.AddOn)  // not in switch->default, for best performance
            {
                keywordFound = false;
                return;
            }
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
                case Token.Result: ResultStatement(); break;
                case Token.Dump: DumpStatement(); break;
                case Token.Call: CallStatement(); break;
                case Token.Chain: ChainStatement(); break;
                case Token.Eval: EvalStatement(); break;
                case Token.ForEach: ForEachStatement(); break;
                case Token.EndForEach: EndForEachStatement(); break;
                case Token.SData: SDataStatement(); break;
                case Token.RInput: RInputStatement(); break;

                default:
                    keywordFound = false;
                    break;
            }
        }

        private void BuildIn_NotAStatement(Token keyword, bool keywordAlreadyFound, out bool doStatement)
        {
            doStatement = false;
            if (!keywordAlreadyFound)
            {
                switch (keyword)
                {
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
                        string msg2 = "";
                        switch (keyword)
                        {
                            case Token.Identifier:
                                msg2 = lex.Identifier;
                                break;
                            case Token.Value:
                                msg2 = lex.Value.ToString();
                                break;
                            default:
                                msg2 = "";
                                break;
                        }
                        Error(Errors.E106_ExpectingKeyword(keyword.ToString(), msg2));
                        break;
                }
            }
            if (lastToken == Token.Colon)
            {
                // we can execute more statements in single line if we use ";"
                while (GetNextToken() == Token.NewLine) { } // GetNextToken but bypass any NewLine
                doStatement = true; // eg: Statement() or StatementAsync() // The caller should do Just that 
            }
        }

    }
}
