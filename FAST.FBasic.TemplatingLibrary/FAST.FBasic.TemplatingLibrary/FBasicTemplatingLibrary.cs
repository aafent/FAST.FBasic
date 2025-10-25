using FAST.FBasic.TemplatingLibrary;

//
// Stream Statements: 
//
// WORDEXTRACTBODY file_stream_name, identifier 
// WORDREPALCEBODY input_stream, output_stream, array
// WORDAPPEND stream_to_append, destination_stream
// WORDPAGEBREAK stream
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
            interpreter.AddStatement("WORDEXTRACTBODY", WordExtractBody);
            interpreter.AddStatement("WORDREPALCEBODY", WordReplaceBody);
            interpreter.AddStatement("WORDAPPEND", WordMergeDocuments);
            interpreter.AddStatement("WORDPAGEBREAK", WordPageBreak);
        }



        private void WordPageBreak(IInterpreter interpreter)
        {
            // WORDPAGEBREAK stream
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string toAppendName = interpreter.lex.Identifier;
            interpreter.GetNextToken();


            var streamToAppend = interpreter.GetVar(toAppendName);
            checkForStream(interpreter, streamToAppend);

            var append = ((Stream)streamToAppend.Object);
            new WordProcessor().AddPageBreak(append);

        }




        private void WordMergeDocuments(IInterpreter interpreter)
        {
            // WORDAPPEND stream_to_append, destination_stream
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string toAppendName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var destName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            var streamToAppend = interpreter.GetVar(toAppendName);
            checkForStream(interpreter, streamToAppend);

            var destStream = interpreter.GetVar(destName);
            checkForStream(interpreter, destStream);

            var dest= ((Stream)destStream.Object);
            var append = ((Stream)streamToAppend.Object);
            new WordDocumentMerger().AppendDocument(dest, append ) ;

        }



        private void WordExtractBody(IInterpreter interpreter)
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

        private void WordReplaceBody(IInterpreter interpreter)
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