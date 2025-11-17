using FAST.AIProvider;

namespace FAST.FBasicInterpreter
{
    /*
    AIChat:
        Statements:

        AIPROVIDER provider_name, OPENAI|CLAUDE|GEMINI|HUGGINGFACE|TEST, *|model     :: setup a AI provider and a model. Use * for the default model.
        AISESSION session_name, provider_name, system_prompt              :: initialize an AI session topic.
        AIPROMPT session_name, variable_response, prompt                  :: Send a prompt over a session and store the response at a variable
        AISETPROVIDER session_name, provider_name                         :: Change the initial provider
     */
    public class FBasicAIChat : IFBasicLibrary
    {
        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddStatement("AIPROVIDER", AIPROVIDER);
            interpreter.AddStatement("AISESSION", AISESSION);
            interpreter.AddStatement("AIPROMPT", AIPROMPT);
            interpreter.AddStatement("AISETPROVIDER", AISETPROVIDER);
        }


        private static void AIPROVIDER(IInterpreter interpreter)
        {
            // Syntax: AIPROVIDER provider_name, OPENAI|CLAUDE|GEMINI|HUGGINGFACE|TEST,  *|model
            //  
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string providerNameToUse = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            Value model;
            if (interpreter.LastToken == Token.Asterisk) 
            {
                model = Value.Wildcard;
            }
            else
            {
                model = interpreter.ValueOrVariable(doMatch: true);
            }
            interpreter.GetNextToken();

            // (v) implementation
            string apiKey ="";
            var request = new FBasicRequestForObjectDescriptor(interpreter, context: "APIKEY", providerNameToUse.ToUpper(), model.String);
            apiKey = interpreter.RequestForObject(request,errorIfNull: true) as string;

            string aiModel = model.String == "*"?null:model.String;
            IAIProvider provider;
            switch (providerNameToUse.ToUpper())
            {
                case "TEST":
                    provider = string.IsNullOrEmpty(aiModel) ? new TestAIProvider(apiKey) : new TestAIProvider(apiKey, aiModel);
                    break;
                case "OPENAI":
                    provider = string.IsNullOrEmpty(aiModel)?new OpenAIProvider(apiKey): new OpenAIProvider(apiKey, aiModel);
                    break;
                case "CLAUDE":
                    provider = string.IsNullOrEmpty(aiModel)?new ClaudeProvider(apiKey): new ClaudeProvider(apiKey, aiModel);
                    break;
                case "GEMINI":
                    provider = string.IsNullOrEmpty(aiModel)?new GeminiProvider(apiKey): new GeminiProvider(apiKey, aiModel);
                    break;
                case "HUGGINGFACE":
                    provider = string.IsNullOrEmpty(aiModel)?new HuggingFaceProvider(apiKey):new HuggingFaceProvider(apiKey, aiModel);
                    break;
                default:
                    interpreter.Error("AIChat", Errors.E106_ExpectingKeyword(providerNameToUse, "Expected: CLAUDE,GEMINI or HUGGINGFACE") );
                    return;
            }

            interpreter.SetVar(name, new Value(provider, $"::AIProvider::{providerNameToUse}"));

        }

        private static void AISESSION(IInterpreter interpreter)
        {
            // Syntax: AISESSION session_name, provider_name, system_prompt
            // 
            interpreter.Match(Token.Identifier);
            string sessionName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string providerName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            var prompt = interpreter.ValueOrVariable(doMatch: true).String;
            interpreter.GetNextToken();

            // (v) implementation
            IAIProvider provider=interpreter.GetVar(providerName).Object as IAIProvider;
            if (string.IsNullOrWhiteSpace(prompt)) prompt = "Use very short responses. Do not provide long analysis texts";
            var session = new AIChatSessionManager(provider, prompt);

            interpreter.SetVar(sessionName,new Value(session, $"::AISESSION::"));

        }


        private static void AIPROMPT(IInterpreter interpreter)
        {
            // Syntax: AIPROMPT session_name, variable_response, prompt
            // 
            interpreter.Match(Token.Identifier);
            string sessionName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string responseVariableName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            var prompt = interpreter.ValueOrVariable(doMatch: true).String;
            interpreter.GetNextToken();

            // (v) implementation
            var session = interpreter.GetVar(sessionName).Object as AIChatSessionManager;
            if (string.IsNullOrWhiteSpace(prompt)) 
            {
                interpreter.SetVar(responseVariableName,Value.Empty);
                return;
            }
            var response = session.SendPromptAsync(prompt).Result;
            interpreter.SetVar(responseVariableName, new Value(response) );
        }

        private static void AISETPROVIDER(IInterpreter interpreter)
        {
            // Syntax: AISETPROVIDER session_name, provider_name
            // 
            interpreter.Match(Token.Identifier);
            string sessionName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string providerName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) implementation
            var session = interpreter.GetVar(sessionName).Object as AIChatSessionManager;
            var provider = interpreter.GetVar(providerName).Object as IAIProvider;

            session.SwitchProvider(provider);
        }


        
    }

}
