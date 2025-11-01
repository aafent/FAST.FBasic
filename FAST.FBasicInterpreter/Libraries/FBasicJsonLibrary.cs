using FAST.FBasicInterpreter.Types;
using System.Dynamic;
using System.Text.Json;

namespace FAST.FBasicInterpreter
{
    /*
    Statements:

    JSON variable_string|value [DYNAMIC] FROM|TO variable_object
        
     */
    public class FBasicJsonLibrary : IFBasicLibrary
    {

        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddStatement("JSON", JsonFromTo);
        }

        private void JsonFromTo(IInterpreter interpreter)
        {
            // Syntax: JSON variable_string|value [DYNAMIC] FROM|TO variable_object
            //

            // (v) argument: name
            interpreter.MatchAny(Token.Value,Token.Identifier);
            var varName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            bool useDynamic = false;
            bool directionFROM = false;
        check_again:
            switch (interpreter.lex.Identifier.ToUpper())
            {
                case "DYNAMIC":
                    useDynamic=true;
                    interpreter.GetNextToken();
                    goto check_again;
                case "TO":
                    directionFROM = false;
                    break;
                case "FROM":
                    directionFROM = true;
                    break;
                default:
                    interpreter.Error("JSON", Errors.E106_ExpectingKeyword(interpreter.lex.Identifier, "Expecting FROM or TO"));
                    return;
            }
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string objName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) implementation 
            bool directionTO = !directionFROM;

            object obj;
            if (directionTO) // (<v) De-Serialize
            {
                var value = interpreter.GetVar(varName);
                if (useDynamic)
                {
                    obj = JsonSerializer.Deserialize<ExpandoObject>(value.String); 
                }
                else
                {
                    if (interpreter.RequestForObjectHandler == null)
                    {
                        interpreter.Error("JSON", Errors.E100_RequestForObjectHandlerNotInstalled(null,"It is necessary for Json DYNAMIC deserialization."));
                        return;
                    }

                    var request=new FBasicRequestForObjectDescriptor(interpreter,"JSON.DESERIALIZE");
                    request.Group=varName;
                    request.Name=objName;
                    obj=null;
                    obj=interpreter.RequestForObjectHandler(request);
                    if (obj == null)
                    {
                        interpreter.Error("JSON",Errors.E128_RequestHandlerNullReturn("JSON.DESERIALIZE", varName, objName,"Expecting an object as result of the deserialization"));
                        return;
                    }
                }
                interpreter.SetVar(objName, new Value(obj,$"Object:{objName}"));
            }
            else if (directionFROM) // (<v) Serialize
            {
                obj = interpreter.GetVar(objName).Object;
                string jsonString;
                if (useDynamic)
                {
                    var eobj=ExpandoConverter.ToDeepExpando(obj);
                    jsonString = jsonString = JsonSerializer.Serialize(eobj);
                }
                else
                {
                    Type runtimeType = obj.GetType();
                    jsonString = JsonSerializer.Serialize(obj, runtimeType);
                }
                interpreter.SetVar(varName,new Value(jsonString));
            }
            return;
        }

    }

}
