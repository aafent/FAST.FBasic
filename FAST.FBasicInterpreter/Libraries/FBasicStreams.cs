//
// Stream Statements: 
//
// FILESTREAM stream_name, in|out, library, path, file_name
// SCOPY source_stream, destination_stream
//
// todo: SREWIND stream_name
// todo: SSEEK stream_name, position
// todo: SCLOSE stream_name
//
// Stream Functions:
//
// todo: slength("stream_name")       :: Return the length of a stream
// todo: sposition("stream_name")     :: Return the current position of a stream
// 

namespace FAST.FBasicInterpreter
{
    public class FBasicStreams : IFBasicLibraryWithMemory, IDisposable
    {
        private const string valueStamp = "_$$Stream$$_";
        private IInterpreter inter;
        //private readonly IFBasicFileManagementLayer fileManager=null;

        public string uniqueName => "FBasicStreams";

        public FBasicStreams()
        {
            //IFBasicFileManagementLayer fileManager=new zzzzzzz();
            //this.fileManager=fileManager;
        }

        public void InstallAll(IInterpreter interpreter)
        {
            this.inter = interpreter;

            interpreter.AddStatement("FILESTREAM", FileStream);
            interpreter.AddStatement("SCOPY", CopyStream);
        }

        private void FileStream(IInterpreter interpreter)
        {
            //
            // Syntax FILESTREAM name, in|out, library, path, file_name
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var direction = interpreter.lex.Identifier.ToUpper();
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            FBasicFileDescriptor file = new();

            interpreter.MatchAny(Token.Identifier, Token.Value);
            file.Library = interpreter.ValueOrIdentifier().String;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.MatchAny(Token.Identifier, Token.Value);
            file.Path = interpreter.ValueOrIdentifier().String;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.MatchAny(Token.Identifier, Token.Value);
            file.FileName= interpreter.ValueOrIdentifier().String;

            interpreter.GetNextToken();

            Value value;
            switch (direction)
            {
                case "IN":
                    // todo: file manager
                    var inputStream = new FileStream(file.FileName, FileMode.Open, FileAccess.Read);
                    value = new Value(inputStream, valueStamp);
                    break;
                case "OUT":
                    // todo: file manager
                    var outputStream = File.Create(file.FileName);
                    value = new Value(outputStream, valueStamp);
                    break;
                default:
                    interpreter.Error("FILESTREAM", Errors.E106_ExpectingKeyword(direction, "Expecting IN or OUT"));
                    return;
            }
            interpreter.SetVar(name,value);

            return;
        }

        private void CopyStream(IInterpreter interpreter)
        {
            //
            // Syntax SCOPY source_name, destination_name
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string sourceName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            string destName = interpreter.lex.Identifier;
            interpreter.GetNextToken();


            var src = interpreter.GetValue(sourceName);
            var dst = interpreter.GetValue(destName);

            checkForStream(interpreter,src);
            checkForStream(interpreter,dst);

            ((Stream)src.Object).CopyTo( ((Stream)dst.Object)   );
            ((Stream)src.Object).Flush();
            ((Stream)dst.Object).Flush();
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
            var variables = inter.GetVariables();
            foreach (var variable in variables)
            {
                if (variable.Value.Type != ValueType.Object) continue;

                if (variable.Value.Object is Stream)
                {
                    var stream = ((Stream)variable.Value.Object);
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        public void Dispose()
        {
            ClearMemory();
        }
    }

}