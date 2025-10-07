namespace FAST.IAIProvider
{
    /// <summary>
    /// Main Session Manager
    /// </summary>
    public class AIChatSessionManager
    {
        private IAIProvider _provider;
        private string _systemPrompt;

        public AIChatSessionManager(IAIProvider provider, string systemPrompt)
        {
            _provider = provider;
            _systemPrompt = systemPrompt;
            _provider.SetSystemPrompt(systemPrompt);
        }

        public async Task<string> SendPromptAsync(string userPrompt)
        {
            return await _provider.SendMessageAsync(userPrompt);
        }

        public void UpdateSystemPrompt(string newSystemPrompt)
        {
            _systemPrompt = newSystemPrompt;
            _provider.SetSystemPrompt(newSystemPrompt);
        }

        public void ClearConversationHistory()
        {
            _provider.ClearHistory();
        }

        public void SwitchProvider(IAIProvider newProvider)
        {
            _provider = newProvider;
            _provider.SetSystemPrompt(_systemPrompt);
        }
    }

}