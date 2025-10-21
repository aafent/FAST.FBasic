namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Helper for the FBasic Interpreter
    /// </summary>
    public static class InterpreterHelper
    {
        /// <summary>
        /// Check if the token is a FBASIC statement
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsStatement(Token token)
        {
            switch (token)
            {
                case Token.Print:
                case Token.If:
                case Token.EndIf:
                case Token.Then:
                case Token.Else:
                case Token.For:
                case Token.To:
                case Token.Next:
                case Token.Goto:
                case Token.Input:
                case Token.Let:
                case Token.Gosub:
                case Token.Return:
                case Token.Rem:
                case Token.End:
                case Token.Assert:
                case Token.AddOn:
                case Token.Result:
                case Token.Dump:
                case Token.Call:
                case Token.Chain:
                case Token.ForEach:
                case Token.EndForEach:
                case Token.SData:
                case Token.RInput:
                    return true;

                default:
                    return false;
            }
            ;

        }

    }
}
