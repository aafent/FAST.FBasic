using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FAST.AIProvider
{

    /// <summary>
    /// Deep Seek Provider
    /// Implementation of IAIProvider for Deep Seek models.
    /// </summary>
    public class DeepSeekProvider : IAIProvider, IAITraceableProvider
    {
        private const string BaseUrl = "https://api.deepseek.com/chat/completions";
        private const string DefaultModel = "deepseek-chat";

        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly List<Message> _messageHistory;
        private string _systemPrompt;
        private object _lastRawResponse;

        public bool IncludeSystemPrompt { get; set; } = true;

        public AITrace trace { get; private set; } = new();

        public DeepSeekProvider(string apiKey, string model = DefaultModel)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

            trace.model=model;
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;
            _httpClient = new HttpClient();
            _messageHistory = new List<Message>();
            _systemPrompt = string.Empty;
        }

        public void SetSystemPrompt(string systemPrompt)
        {
            _systemPrompt = systemPrompt ?? string.Empty;
        }

        public void ClearHistory()
        {
            _messageHistory.Clear();
        }

        public async Task<string> SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            // Add user message to history
            _messageHistory.Add(new Message { Role = "user", Content = message });

            // Build messages array for API request
            var messages = new List<Message>();

            // Add system prompt if enabled and set
            if (IncludeSystemPrompt && !string.IsNullOrWhiteSpace(_systemPrompt))
            {
                messages.Add(new Message { Role = "system", Content = _systemPrompt });
            }

            // Add conversation history
            messages.AddRange(_messageHistory);

            // Create request payload
            var requestPayload = new DeepSeekRequest
            {
                Model = _model,
                Messages = messages,
                Stream = false
            };

            // Serialize request
            var jsonContent = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            trace.request=jsonContent;

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Set authorization header
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            // Send request
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Read and parse response
            var responseContent = await response.Content.ReadAsStringAsync();
            trace.response = responseContent;

            var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            // Store raw response
            _lastRawResponse = deepSeekResponse;

            // Extract assistant message
            var assistantMessage = deepSeekResponse?.Choices?[0]?.Message?.Content ?? string.Empty;

            // Add assistant response to history
            _messageHistory.Add(new Message { Role = "assistant", Content = assistantMessage });

            return assistantMessage;
        }

        #region Private Classes for JSON Serialization

        private class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        private class DeepSeekRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("messages")]
            public List<Message> Messages { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        private class DeepSeekResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            [JsonPropertyName("usage")]
            public Usage Usage { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public Message Message { get; set; }

            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; }
        }

        private class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        #endregion
    }
}