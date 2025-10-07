namespace FAST.IAIProvider
{
    /// <summary>
    /// Trace record of AI provider
    /// </summary>
    public class AITrace
    {
        /// <summary>
        /// The requested model
        /// </summary>
        public string model {  get; set; }

        /// <summary>
        /// The request json 
        /// </summary>
        public string request { get; set; }

        /// <summary>
        /// The response received.
        /// </summary>
        public string response {  get; set; }

    }
}
