using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FAST.AIProvider
{
    /// <summary>
    /// OpenAI GPT Provider
    /// Implementation of IAIProvider for OpenAI's GPT models.
    /// </summary>
    public class OpenAIProvider : IAIProvider, IAITraceableProvider
    {
        private readonly HttpClient _client;
        private readonly string _model;
        private readonly List<OpenAIMessage> _messages;
        public AITrace trace { get; private set; } = new();

        public OpenAIProvider(string apiKey, string model = "gpt-3.5-turbo")
        {
            trace.model=model;
            _model = model;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _messages = new List<OpenAIMessage>();
        }

        public void SetSystemPrompt(string systemPrompt)
        {
            _messages.Clear();
            _messages.Add(new OpenAIMessage { Role = "system", Content = systemPrompt });
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                _messages.Add(new OpenAIMessage { Role = "user", Content = message });

                var request = new
                {
                    model = _model,
                    messages = _messages.Select(m => new { role = m.Role, content = m.Content }).ToList()
                };

                var json = JsonSerializer.Serialize(request);
                trace.request=json;

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    content
                );

                var responseBody = await response.Content.ReadAsStringAsync();
                trace.response= responseBody;

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"OpenAI API error ({response.StatusCode}): {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var assistantMessage = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response";

                _messages.Add(new OpenAIMessage { Role = "assistant", Content = assistantMessage });
                return assistantMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"OpenAI request failed: {ex.Message}", ex);
            }
        }

        public void ClearHistory()
        {
            var systemMessage = _messages.FirstOrDefault(m => m.Role == "system");
            _messages.Clear();
            if (systemMessage != null)
                _messages.Add(systemMessage);
        }

        private class OpenAIMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }
    }

}