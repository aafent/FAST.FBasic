
namespace FAST.FBasicInterpreter
{
    public class InterpretationState
    {
        public LexerState lexerState { get; set; }

        public Token prevToken { get; set; }
        public Marker lineMarker {  get; set; }
        public Token lastToken {get; set; } // last seen token

    }
}
