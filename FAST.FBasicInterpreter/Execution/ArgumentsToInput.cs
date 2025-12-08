namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Helper class to convert method arguments to input strings for the interpreter
    /// Used as input handler in the execution environment and the interpreter
    /// </summary>
    public class ArgumentsToInput
    {
        private object[] args;
        private int currentArgIndex = -1;

        /// <summary>
        /// Creates a new instance of ArgumentsToInput
        /// </summary>
        /// <param name="args">Arguments one by one respecting the sequence of program inputs</param>
        public ArgumentsToInput(params object[] args)
        {
            this.args = args;
        }

        /// <summary>
        /// Returns the next argument as string input for the interpreter
        /// Used as input handler in the execution environment and the interpreter
        /// <example>
        /// env.inputHandler = () => args.GetNextArgumentAsInput();
        /// </example>
        /// </summary>
        public string? GetNextArgumentAsInput()
        {
            if (currentArgIndex + 1 == args.Length) return null;
            currentArgIndex++;
            switch (args[currentArgIndex])
            {
                case string s:
                    return s;
                case int i:
                    return i.ToString();
                case double d:
                    return d.ToString();
                case DateTime dt:
                    var sdt = dt.ToString(Value.dateFormat);
                    return sdt;
                default:
                    throw new NotImplementedException($"Type {args[currentArgIndex].GetType().ToString()} not implemented");
            }
        }
    }
}
