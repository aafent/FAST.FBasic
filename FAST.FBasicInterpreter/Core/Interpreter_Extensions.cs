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


        /// <summary>
        /// Get a value of any name (variable, collection item)
        /// </summary>
        /// <param name="interpreter">the interpreter</param>
        /// <param name="name">The name</param>
        /// <returns>Value</returns>
        public static Value GetValue(this Interpreter interpreter, string name)
        {
            string firstPart = null;
            string otherPart = null;
            if (name.Length>1 && name[0]=='[' && name[name.Length-1]==']')
            {
                // (v) remove the surrounding []
                name=name.Substring(1,name.Length-2);
            }
            var dotPosition = name.IndexOf('.');
            if (dotPosition >= 0) // (v) collections dotted identifier [collection.item]
            {
                firstPart = name.Substring(0, dotPosition);
                otherPart = name.Substring(dotPosition + 1);

                if (interpreter.collections.ContainsKey(firstPart))
                {
                    return interpreter.collections[firstPart].getValue(otherPart);
                }
                else
                {
                    return interpreter.Error(null,$"Undeclared name {name} [E111]").value;
                }
            } 
            else
            {
                return interpreter.GetVar(name);
            }

        }


        /// <summary>
        /// Dump the source line at the Market, for debugging purposes 
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="marker"></param>
        public static void DumpSourceLine(this Interpreter interpreter, Marker marker)
        {
            string line = interpreter.lex.GetLine(marker.Line);
            if (!string.IsNullOrEmpty(line))
            {
                if (marker.Column >= 0 && marker.Column <= line.Length)
                {
                    line = $"L{marker.Line}: " + line.Insert(marker.Column, $"[<-({marker.Column})-]");
                }
            }
            Console.WriteLine(line);
        }

    }
}
