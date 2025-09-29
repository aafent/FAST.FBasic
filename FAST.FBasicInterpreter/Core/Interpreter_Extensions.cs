using System.Runtime.CompilerServices;

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
        /// <param name="interpreter">The interpreter</param>
        /// <param name="copyOfTheVariables">Optional,if true return a copy of the variables otherwise a reference to them</param>
        /// <returns>the results</returns>
        public static ExecutionResult ExecWithResult(this Interpreter interpreter,bool copyOfTheVariables=false)
        {
            ExecutionResult result = new();
            try
            {
                interpreter.Exec();
                result.hasError = false;
                result.value = interpreter.Result;
                if (copyOfTheVariables) // (v) copy of the variables
                {
                    var variables=interpreter.GetVariables();
                    result.variables=new();
                    foreach (var item in variables)
                    {
                        result.variables.Add(item.Key, new Value(item.Value));
                    }
                } else
                {
                    result.variables = interpreter.GetVariables(); // Reference to the variables
                }
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
