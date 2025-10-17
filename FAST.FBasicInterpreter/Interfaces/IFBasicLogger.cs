namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Interface for FBASIC Logger
    /// </summary>
    public interface IFBasicLogger
    {
        /// <summary>
        /// Log an debug text
        /// </summary>
        /// <param name="text">Text to log</param>
        void debug(string text);

        /// <summary>
        /// Log an info text
        /// </summary>
        /// <param name="text">Text to log</param>
        void info(string text);

        /// <summary>
        /// Log an error text
        /// </summary>
        /// <param name="text">Text to log</param>
        void error(string text);
    }
}