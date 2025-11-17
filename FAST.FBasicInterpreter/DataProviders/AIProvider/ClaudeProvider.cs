using System.Text;
using System.Text.Json;

namespace FAST.AIProvider
{
    /// <summary>
    /// Anthropic Claude Provider
    /// Implementation of IAIProvider for Anthropic's Claude model.
    /// </summary>
    public class ClaudeProvider : IAIProvider, IAITraceableProvider
    {
        private readonly HttpClient _client;
        private readonly string _model;
        private readonly List<ClaudeMessage> _messages;
        private string _systemPrompt = "";
        
        public AITrace trace { get; private set;} = new();  


        public ClaudeProvider(string apiKey, string model = "claude-sonnet-4-5-20250929")
        {
            trace.model=model;
            _model = model;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _messages = new List<ClaudeMessage>();
        }


        public void SetSystemPrompt(string systemPrompt)
        {
            _systemPrompt = systemPrompt;
            _messages.Clear();
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                _messages.Add(new ClaudeMessage { Role = "user", Content = message });

                var requestObj = new
                {
                    model = _model,
                    max_tokens = 4096,
                    messages = _messages.Select(m => new { role = m.Role, content = m.Content }).ToList()
                };

                // Add system prompt if present
                object finalRequest;
                if (!string.IsNullOrEmpty(_systemPrompt))
                {
                    finalRequest = new
                    {
                        model = requestObj.model,
                        max_tokens = requestObj.max_tokens,
                        system = _systemPrompt,
                        messages = requestObj.messages
                    };
                }
                else
                {
                    finalRequest = requestObj;
                }

                var json = JsonSerializer.Serialize(finalRequest);
                trace.request=json;

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(
                    "https://api.anthropic.com/v1/messages",
                    content
                );

                var responseBody = await response.Content.ReadAsStringAsync();
                trace.response=responseBody;

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Claude API error ({response.StatusCode}): {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var assistantMessage = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response";

                _messages.Add(new ClaudeMessage { Role = "assistant", Content = assistantMessage });
                return assistantMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Claude request failed: {ex.Message}", ex);
            }
        }

        public void ClearHistory()
        {
            _messages.Clear();
        }

        private class ClaudeMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }
    }

}