namespace FAST.FBasicInterpreter
{
    public class fBasicException : Exception
    {
        public int line;
        public int column;
        public int pointer;
        public string sourceLine=null;

        public fBasicException() : base()
        {
            this.line = -1;
            this.column = -1;
            this.pointer = -1;
            this.sourceLine=null;
        }

        public fBasicException(string message, int line):base(message)
        {
            this.line = line;
        }
        public fBasicException(string message, Marker marker): base(message)
        {
            this.line = marker.Line;
            this.column =marker.Column;
            this.pointer = marker.Pointer;
        }
        public fBasicException(string message, Marker marker, string sourceLine): this(message,marker)
        {
            this.sourceLine=sourceLine;
        }

        public fBasicException(string message, int line, Exception inner)
            : base(message, inner)
        {
            this.line = line;
        }

        public override string ToString()
        {
            string msg=null;
            string msg2="";
            if (this.line > 0)
                msg = $"ERROR At line:{this.line}, {this.Message}.";
            else
                msg =$"ERROR {this.Message}.";
           
            if (column>=0 ) msg2=$"C{column}";
            if (pointer>=0)
            {
                if (!string.IsNullOrEmpty(msg2)) msg2+=":";
                msg2+=$"P{pointer}";
            }
            if (!string.IsNullOrEmpty(msg2)) msg+=$" ({msg2})";

            return msg;
        }
    }
}
