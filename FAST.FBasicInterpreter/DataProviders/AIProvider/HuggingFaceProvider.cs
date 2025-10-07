using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FAST.IAIProvider
{
    /// <summary>
    /// Hugging Face Inference Providers API (New 2025 API)
    /// </summary>
    public class HuggingFaceProvider : IAIProvider, IAITraceableProvider
    {
        private readonly HttpClient _client;
        private readonly string _model;
        private readonly List<HFMessage> _messages;
        private string _systemPrompt = "";
        public AITrace trace { get; private set; } = new();

        public HuggingFaceProvider(string apiKey, string model = "meta-llama/Llama-3.3-70B-Instruct")
        {
            trace.model = model;
            _model = model;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _messages = new List<HFMessage>();
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
                // Build messages with system prompt
                var apiMessages = new List<object>();

                if (!string.IsNullOrEmpty(_systemPrompt))
                {
                    apiMessages.Add(new { role = "system", content = _systemPrompt });
                }

                // Add conversation history
                apiMessages.AddRange(_messages.Select(m => new { role = m.Role, content = m.Content }));

                // Add current user message
                apiMessages.Add(new { role = "user", content = message });
                _messages.Add(new HFMessage { Role = "user", Content = message });

                var request = new
                {
                    model = _model,
                    messages = apiMessages,
                    max_tokens = 1024,
                    stream = false
                };

                var json = JsonSerializer.Serialize(request);
                trace.request= json;

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use the NEW Inference Providers API (router endpoint)
                var response = await _client.PostAsync(
                    "https://router.huggingface.co/v1/chat/completions",
                    content
                );

                var responseBody = await response.Content.ReadAsStringAsync();
                trace.response=responseBody;

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HuggingFace API error ({response.StatusCode}): {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var assistantMessage = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response";

                _messages.Add(new HFMessage { Role = "assistant", Content = assistantMessage });
                return assistantMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"HuggingFace request failed: {ex.Message}", ex);
            }
        }

        public void ClearHistory()
        {
            _messages.Clear();
        }

        private class HFMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }
    }

}