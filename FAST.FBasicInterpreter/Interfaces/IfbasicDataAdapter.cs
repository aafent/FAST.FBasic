namespace FAST.FBasicInterpreter
{

    /// <summary>
    /// Data Provider for the FBASIC
    /// </summary>
    public interface IFBasicDataAdapter : IFBasicError
    {
        /// <summary>
        /// a unique name of the data adapter
        /// </summary>
        string name { get; }


        /// <summary>
        /// Bind the adapter with the interpreter.
        /// This is the place to add functions, statements and data providers to the interpreter
        /// </summary>
        /// <param name="interpreter">A reference to the interpreter</param>
        void bind(Interpreter interpreter);

    }

}
