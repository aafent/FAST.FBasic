
namespace FAST.AIProvider
{
    /// <summary>
    /// Base interface for all AI providers
    /// </summary>
    public interface IAIProvider
    {
        Task<string> SendMessageAsync(string message);
        void SetSystemPrompt(string systemPrompt);
        void ClearHistory();
    }
}