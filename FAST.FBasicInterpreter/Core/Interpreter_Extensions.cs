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


        /// <summary>
        /// Execute the program and return the results
        /// </summary>
        /// <returns>the results</returns>
        public static ExecutionResult ExecWithResult(this Interpreter interpreter)
        {
            ExecutionResult result = new();
            try
            {
                interpreter.Exec();
                result.hasError = false;
                result.value = interpreter.Result;
            }
            catch (fBasicException e)
            {
                result.hasError = true;
                result.errorText = e.ToString();
                result.lineOfError = e.line;
                result.errorSourceLine = e.sourceLine;
            }
            return result;
        }



    }
}
