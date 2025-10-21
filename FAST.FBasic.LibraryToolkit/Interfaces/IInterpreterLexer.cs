namespace FAST.FBasicInterpreter
{
    public interface IInterpreterLexer
    {
        string AddOn { get; set; }
        Marker CurrentSourceMarker { get; }
        string Identifier { get; set; }
        char lastChar { get; }
        Marker TokenMarker { get; set; }
        Value Value { get; set; }
        string GetLine(int lineNumber);
        string GetLine(Marker marker);
        string GetSource();
        LexerState GetState();
        Token GetToken();
        void GoTo(Marker marker);
        void RestartProgram();
        void SetAddonStatements(string[] names);
        void SetLastChar(char value);
        void SetSource(string sourceProgram);
        void SetState(LexerState state);
    }
}