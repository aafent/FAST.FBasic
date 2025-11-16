namespace FAST.FBasicInterpreter
{
    /*
    Events:
        Statements:

        RAISEEVENT event_name [, parameter1, parameter2, ...]	Triggers an event named event_name and optionally passes data (parameters) to any registered handlers. 
        ERROR RESULTVALUE, error_message
     */
    public class FBasicEvents : IFBasicLibrary
    {
        // Declare the event using the built-in EventHandler<T> generic delegate
        //    The signature is: void (object sender, (name, Stack<Value> args) e)
        //       where sender: is the interpreter, e.name: is the event name and e.args: are the arguments
        public static event EventHandler<(string,Stack<Value>)> FBasicEventHandler = null;


        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddStatement("RAISEEVENT", RaiseEvent);
            interpreter.AddStatement("ERROR", RaiseError);
        }


        /// <summary>
        /// enable or disable the event's triggering
        /// </summary>
        public static bool Enabled {  get; set; } = true;

        

        /// <summary>
        /// Reset the FBasicEventHandler
        /// </summary>
        public static void Reset()
        {
            Enabled = true;
            FBasicEventHandler=null;
        }

        private static void RaiseEvent(IInterpreter interpreter)
        {
            // Syntax: RAISEEVENT event_name [, parameter1,p2,p3...]
            //  

            Stack<Value> args = new();

            interpreter.Match(Token.Identifier);
            string eventName = interpreter.lex.Identifier;

            bool doLoop=true;
            while (doLoop)
            {
                interpreter.GetNextToken(); // move to next token

                switch (interpreter.LastToken)
                {
                    case Token.Comma:
                        continue; // fetch next token

                    case Token.NewLine:
                        doLoop=false;
                        break;
                    case Token.Identifier:
                        var value= interpreter.GetIdentifierOrCF(true,true,false);
                        args.Push(value);
                        break;

                    case Token.Value:
                        args.Push(interpreter.ValueOrVariable());
                        break;
                    default:
                        break;
                }
                continue; // next Token
            }

            if (FBasicEventHandler!=null)
            {
                if (Enabled) FBasicEventHandler.Invoke(interpreter, (eventName, args));
            }
            
        }

        private static void RaiseError(IInterpreter interpreter)
        {
            //Syntax: Error RESULTVALUE, error_message
            interpreter.SetVar("RESULTVALUE",interpreter.ValueOrVariable(true));

            interpreter.GetNextToken();
            interpreter.MatchAndThenNextToken(Token.Comma);

            var msg= interpreter.ValueOrVariable(true).ToString();

            interpreter.Error($"",Errors.E001_UserError(msg));
            return;

        }


    }

}
