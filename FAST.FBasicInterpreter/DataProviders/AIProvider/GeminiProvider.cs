using System.Text;
using System.Text.Json;

namespace FAST.AIProvider
{
    // Google Gemini Provider
    public class GeminiProvider : IAIProvider, IAITraceableProvider
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly List<GeminiContent> _history;
        private string _systemPrompt = "";
        public AITrace trace { get; private set; } = new();

        public GeminiProvider(string apiKey, string model = "gemini-2.5-flash")
        {
            trace.model=model;
            _apiKey = apiKey;
            _model = model;
            _client = new HttpClient();
            _history = new List<GeminiContent>();
        }

        public void SetSystemPrompt(string systemPrompt)
        {
            _systemPrompt = systemPrompt;
            _history.Clear();
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                // If this is the first message and we have a system prompt, prepend it
                if (_history.Count == 0 && !string.IsNullOrEmpty(_systemPrompt))
                {
                    var combinedMessage = $"{_systemPrompt}\n\nUser: {message}";
                    _history.Add(new GeminiContent
                    {
                        Role = "user",
                        Parts = new[] { new GeminiPart { Text = combinedMessage } }
                    });
                }
                else
                {
                    _history.Add(new GeminiContent
                    {
                        Role = "user",
                        Parts = new[] { new GeminiPart { Text = message } }
                    });
                }

                var requestObj = new
                {
                    contents = _history.Select(h => new
                    {
                        role = h.Role,
                        parts = h.Parts.Select(p => new { text = p.Text }).ToArray()
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestObj);
                trace.request=json;

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}",
                    content
                );

                var responseBody = await response.Content.ReadAsStringAsync();
                trace.response=responseBody;

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var assistantMessage = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response";

                _history.Add(new GeminiContent
                {
                    Role = "model",
                    Parts = new[] { new GeminiPart { Text = assistantMessage } }
                });

                return assistantMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gemini request failed: {ex.Message}", ex);
            }
        }

        public void ClearHistory()
        {
            _history.Clear();
        }

        private class GeminiContent
        {
            public string Role { get; set; } = "";
            public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
        }

        private class GeminiPart
        {
            public string Text { get; set; } = "";
        }
    }
}