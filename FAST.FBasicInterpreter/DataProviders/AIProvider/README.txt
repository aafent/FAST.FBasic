/*
 * Important: Replace the placeholder API keys with your actual keys from:

OpenAI: https://platform.openai.com/api-keys
Gemini: https://makersuite.google.com/app/apikey
Claude: https://console.anthropic.com/
Hugging Face: https://huggingface.co/settings/profile

The code handles JSON serialization, HTTP requests, conversation state, 
and error handling automatically!
*/

Example code:

var systemPrompt = "You are an Oceanographer. Provide clear and concise responses.";


Console.WriteLine("=== Testing HuggingFace ===");
var hfKey = Environment.GetEnvironmentVariable("HF_API_KEY") ?? "your-key-here";
var hf = new HuggingFaceProvider(hfKey, "meta-llama/Llama-3.3-70B-Instruct");
var session = new AIChatSessionManager(hf, systemPrompt);
var resp = await session.SendPromptAsync("Write a text of 200 words about the sea polution");
Console.WriteLine($"Response: {resp4}\n");