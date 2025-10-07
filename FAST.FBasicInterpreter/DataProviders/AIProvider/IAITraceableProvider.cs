namespace FAST.IAIProvider
{
    /// <summary>
    /// Traceable AI provider interface
    /// </summary>
    public interface IAITraceableProvider
    {
        /// <summary>
        /// Trace record of the provider
        /// </summary>
        AITrace trace { get; }
    }

}
