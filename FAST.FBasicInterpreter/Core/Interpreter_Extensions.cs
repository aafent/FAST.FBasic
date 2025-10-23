using System.Reflection.Emit;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Extensions to the class Interpreter 
    /// </summary>
    public static class Interpreter_Extensions
    {
        /// <summary>
        /// Add a library with Functions and/or Statements
        /// If the library already exists, nothing will done. 
        /// </summary>
        /// <param name="interpreter">The interpreter</param>
        /// <param name="library">The library to install</param>
        public static void AddLibrary(this Interpreter interpreter, IFBasicLibrary library)
        {
            library.InstallAll(interpreter);
            if (library is IFBasicLibraryWithMemory)
            {
                if (interpreter.librariesWithMemory == null) interpreter.librariesWithMemory = new();
                var lib=(IFBasicLibraryWithMemory)library;
                if (!interpreter.librariesWithMemory.ContainsKey(lib.uniqueName))
                {
                    interpreter.librariesWithMemory.Add(lib.uniqueName, lib);
                }
            }
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
                result.programStartWhen= DateTime.Now;
                interpreter.Exec();
                result.programEndWhen = DateTime.Now;
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
            catch (FBasicException e)
            {
                result.hasError = true;
                result.errorText = e.ToString();
                result.lineOfError = e.line;
                result.errorSourceLine = e.sourceLine;
                #if DEBUG
                result.exception=e;
                #endif
            }
            catch (IndexOutOfRangeException e1)
            {
                result.hasError = true;
                result.errorText = Errors.E130_OutOfRange(interpreter.lex.Identifier);
                result.lineOfError = interpreter.lex.CurrentSourceMarker.Line;
                result.errorSourceLine = interpreter.lex.GetLine(result.lineOfError);
                #if DEBUG
                result.exception = e1;
                #endif
            }
            catch (Exception ex)
            {
                result.hasError = true;
                result.errorText = ex.ToString();
                result.lineOfError = interpreter.lex.CurrentSourceMarker.Line;
                result.errorSourceLine = interpreter.lex.GetLine(result.lineOfError);
                #if DEBUG
                result.exception = ex;
                #endif
            }
            finally
            {
                foreach (var lib in interpreter.librariesWithMemory)
                {
                    if (lib.Value is IDisposable)
                    {
                        try // do not produce errors from Dispose
                        {
                            ((IDisposable)lib.Value).Dispose();
                        }
                        catch
                        {
                        }

                    }
                }

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
            var parser=new IdentifierNotationParser(name,  interpreter);
            if (parser.IsArray)
            {
                return interpreter.GetArray(parser.DataContainerName)[parser.ArrayIndex, parser.DataContainerName];
            } 
            else if (parser.IsCollection )
            {
                return interpreter.collections[parser.DataContainerName].getValue(parser.DataElement);
            } 
            else
            {
                return interpreter.GetVar(name);
            }

        }

        /// <summary>
        /// Check if a name is collection or array
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        public static bool IsCollectionOrArray(this Interpreter interpreter, string name)
        {
            return interpreter.IsCollection(name) || interpreter.IsArray(name);
        }

        /// <summary>
        /// Return the basic collection for the array or the collection
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IBasicCollection GetCollectionOrArray(this Interpreter interpreter, string name)
        {
            if (interpreter.IsCollection(name)) return interpreter.GetCollection(name);
            return interpreter.GetArray(name);
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
