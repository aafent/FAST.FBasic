namespace FAST.FBasicInterpreter
{

    /// <summary>
    /// A marker (location) in the source program
    /// </summary>
    public struct Marker
    {
        /// <summary>
        /// Pointer is a character position in the source program.
        /// Start from 0 and ends at the program's length
        /// </summary>
        public int Pointer { get; set; }

        /// <summary>
        /// The line of the source program
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Column in a  character in a line, 
        /// starts from 1 and end to the length of the line
        /// </summary>
        public int Column { get; set; }

        public Marker(int pointer, int line, int column): this()
        {
            Pointer = pointer;
            Line = line;
            Column = Column;
        }


        public Marker(Marker marker) : this()
        {
            this.Pointer = marker.Pointer;
            this.Line = marker.Line;
            this.Column = marker.Column;
        }

        public Marker OnePointBack()
        {
            //lex.GoTo(new Marker(loops[endForEachLoop].Pointer - 1,
            //             loops[endForEachLoop].Line, loops[endForEachLoop].Column - 1));
            var beginOfStatement = new Marker(this);
            beginOfStatement.Pointer --;
            beginOfStatement.Column --; 
            return beginOfStatement;
        }

    }
}
