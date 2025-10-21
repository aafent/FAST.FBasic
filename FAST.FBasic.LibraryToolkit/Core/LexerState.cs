
namespace FAST.FBasicInterpreter
{
    public class LexerState
    {
        public string Source { get; set; }
        public Marker SourceMarker {get; set; }
        public char LastChar { get; set; }
        public Marker TokenMarker { get; set; }

        public string Identifier { get; set; } // Last encountered identifier
        public Value Value { get; set; } // Last number or string
        public string AddOn { get; set; } // Last AddOn Statement found

    }
}
