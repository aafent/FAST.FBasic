namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Abstract class for FBasic logger implementation
    /// </summary>
    public abstract class FBasicLoggerAbstract : IFBasicLogger
    {
        protected string logPrefix(string severity)
        {
            return $"{severity}:\t";
        }

        /// <summary>
        /// Write a line to the Console
        /// </summary>
        /// <param name="line">the text to write</param>
        protected virtual void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        /// <summary>
        /// Log an info text
        /// </summary>
        /// <param name="text">Text to log</param>
        public void info(string text)
        {
            this.WriteLine(logPrefix("INFO") + text);
        }

        /// <summary>
        /// Log an debug text
        /// </summary>
        /// <param name="text">Text to log</param>
        public void debug(string text)
        {
            this.WriteLine(logPrefix("DEBUG") + text);
        }

        public void error(string text)
        {
            this.WriteLine(logPrefix("ERROR") + text);
        }
    }

    /// <summary>
    /// Internal class, used as the built-in console logger
    /// </summary>
    internal sealed class defaultExecutionLogger : FBasicLoggerAbstract
    { }


}
