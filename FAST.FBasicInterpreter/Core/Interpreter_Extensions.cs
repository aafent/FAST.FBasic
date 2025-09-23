namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Extensions to the class Interpreter 
    /// </summary>
    public static class Interpreter_Extensions
    {
        /// <summary>
        /// Add a library with Functions and/or Statements
        /// </summary>
        /// <param name="interpreter">The interpreter</param>
        /// <param name="library">The library to install</param>
        public static void AddLibrary(this Interpreter interpreter, IFBasicLibrary library)
        {
            library.InstallAll(interpreter);
        }

    }
}
