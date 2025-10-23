using FAST.FBasic.TemplatingLibrary;

//
// Stream Statements: 
//
// EXTRACTWORDBODY file_stream_name, identifier 
// REPALCEWORDBODY input_stream, output_stream, array
//

namespace FAST.FBasicInterpreter.TemplatingLibrary
{
    public class FBasicTemplatingLibrary  : IFBasicLibraryWithMemory
    {
        private const string valueStamp = "_$$Stream$$_";
        private IInterpreter inter;
        //private readonly IFBasicFileManagementLayer fileManager=null;

        public string uniqueName => "TemplatingLibrary";

        public FBasicTemplatingLibrary()
        {
            //IFBasicFileManagementLayer fileManager=new zzzzzzz();
            //this.fileManager=fileManager;
        }


        public void InstallAll(IInterpreter interpreter)
        {
            this.inter = interpreter;
            interpreter.AddStatement("EXTRACTWORDBODY", ExtractWordBody);
            interpreter.AddStatement("REPALCEWORDBODY", ReplaceWordBody);

        }

        private void ExtractWordBody(IInterpreter interpreter)
        {
            //
            // Syntax: EXTRACTWORDBODY file_stream_name, identifier 
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();


            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var variableToSave = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            var stream=interpreter.GetVar(name);
            checkForStream(interpreter, stream);

            var text= new WordProcessor().ExtractAllText(((Stream)stream.Object));

            interpreter.SetVar(variableToSave,new Value(text) );
        }

        private void ReplaceWordBody(IInterpreter interpreter)
        {
            //
            // Syntax: REPALCEWORDBODY input_stream, output_stream, array
            //
            // (v) argument: input_stream
            interpreter.Match(Token.Identifier);
            string inputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            // (v) argument: output_stream
            interpreter.Match(Token.Identifier);
            string outputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            // (v) argument: array
            interpreter.Match(Token.Identifier);
            var arrayName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) error checking

            if (!interpreter.IsArray(arrayName) )
            {
                interpreter.Error("REPALCEWORDBODY",Errors.E112_UndeclaredEntity("Array",arrayName));
                return;
            }

            var stream = interpreter.GetVar(inputName);
            checkForStream(interpreter, stream);

            var array = interpreter.GetArray(arrayName);

            // (v) implementation
            var replacements=array.ConvertToDictionary<string,string>(0,1);
            foreach (var item in replacements)
            {
                replacements[item.Key] = ToolKitHelper.FormatStringValue(item.Key, interpreter.GetValue(item.Value).ToString());
            }
            var mStream = new WordProcessor().ReplacePlaceholders(((Stream)stream.Object), replacements, "{", "}");

            interpreter.SetVar(outputName, new Value(mStream,valueStamp));
        }



        private void checkForStream(IInterpreter interpreter, Value value)
        {
            if (value.Type != ValueType.Object)
            {
                interpreter.Error("STREAM",Errors.E127_WrongArgumentReferredType("STREAM"));
                return;
            }
        }

        public void ClearMemory()
        {
        }
    }

}