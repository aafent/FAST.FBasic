namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Extensions to the class Interpreter 
    /// </summary>
    public static class Interpreter_ToolKitExtensions
    {
        /// <summary>
        /// Add a library with Functions and/or Statements
        /// If the library already exists, nothing will done. 
        /// </summary>
        /// <param name="interpreter">The interpreter</param>
        /// <param name="library">The library to install</param>
        public static void AddLibrary(this IInterpreter interpreter, IFBasicLibrary library)
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
        /// Check if a name is collection or array
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        public static bool IsCollectionOrArray(this IInterpreter interpreter, string name)
        {
            return interpreter.IsCollection(name) || interpreter.IsArray(name);
        }

        /// <summary>
        /// Return the basic collection for the array or the collection
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IBasicCollection GetCollectionOrArray(this IInterpreter interpreter, string name)
        {
            if (interpreter.IsCollection(name)) return interpreter.GetCollection(name);
            return interpreter.GetArray(name);
        }

        /// <summary>
        /// Dump the source line at the Market, for debugging purposes 
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="marker"></param>
        public static void DumpSourceLine(this IInterpreter interpreter, Marker marker)
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


        /// <summary>
        /// Match any of the input tokens, return the matching token or create error
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="tokens">array of at least one token</param>
        /// <returns>Matched token</returns>
        public static Token MatchAndThenNextToken(this IInterpreter interpreter, params Token[] tokens)
        {
            var last=interpreter.LastToken;
            foreach (var token in tokens)
            {
                if (token == last)
                {
                    interpreter.GetNextToken();
                    return token;
                }
            }
            interpreter.Match(tokens[0]); // will create error
            return tokens[0];
        }

        /// <summary>
        /// Match any of the tokens or error
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="tokens"></param>
        /// <returns>Token that match</returns>
        public static Token MatchAny(this IInterpreter interpreter, params Token[] tokens)
        {
            var last = interpreter.LastToken;
            foreach (var token in tokens)
            {
                if (token == last)
                {
                    return token;
                }
            }
            interpreter.Match(tokens[0]); // will create error
            return tokens[0];
        }

    }
}
