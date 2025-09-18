namespace FAST.FBasicInterpreter
{
    public abstract class executionLoggerAbstract
    {
        protected string logPrefix(string severity)
        {
            return $"{severity}:\t";
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public void info(string text)
        {
            this.WriteLine(logPrefix("INFO")+text);
        }

        public void debug(string text)
        {
            this.WriteLine(logPrefix("DEBUG") + text);
        }
    }

    internal sealed class defaultExecutionLogger : executionLoggerAbstract
    { }


}
