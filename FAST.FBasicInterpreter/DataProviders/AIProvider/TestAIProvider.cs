namespace FAST.AIProvider
{
    // Test Provider
    public class TestAIProvider : IAIProvider, IAITraceableProvider
    {
        public AITrace trace { get; private set; } = new();

        public TestAIProvider(string apiKey, string model = "test")
        {
            trace.model=model;
        }

        public void SetSystemPrompt(string systemPrompt)
        {
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            { 

                var msg= "Whales are magnificent, diverse marine mammals, not fish. As warm-blooded, air-breathing vertebrates, they are highly adapted to life in the ocean, found in all major oceans from polar to tropical waters.\r\n\r\nThere are two main groups:\r\n1.  **Baleen whales:** Filter feeders that strain tiny organisms like krill and small fish from the water.\r\n2.  **Toothed whales:** Active predators that hunt fish, squid, and other marine mammals using echolocation.\r\n\r\nWhales are intelligent creatures, exhibiting complex social behaviors, vocalizations, and incredible migratory patterns. They play crucial roles in maintaining healthy marine ecosystems. Many species face significant conservation challenges.";

                return msg;
            }
            catch (Exception ex)
            {
                throw new Exception($"TEST-AI-Provider request failed: {ex.Message}", ex);
            }
        }

        public void ClearHistory()
        {
        }

    }
}