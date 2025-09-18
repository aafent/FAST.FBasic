namespace FAST.FBasicInterpreter
{

    /// <summary>
    /// Data Provider for the FBASIC
    /// </summary>
    public interface IfbasicDataAdapter : IfbasicError
    {
        /// <summary>
        /// a unique name of the data adapter
        /// </summary>
        string name { get; }


        /// <summary>
        /// Bind the adapter with the interpreter
        /// </summary>
        /// <param name="interpreter">A reference to the interpreter</param>
        void bind(Interpreter interpreter);

    }

}
