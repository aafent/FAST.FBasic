//
// Stream Statements: 
//
// FILESTREAM stream_name, in|inmemory|out, library, path, file_name
// SCOPY source_stream, destination_stream
// MEMSTREAM stream_name
// SREWIND stream_name
//
// todo: SSEEK stream_name, position
// todo: SCLOSE stream_name
//
// Stream Functions:
//
// slength("stream_name")       :: Return the length of a stream
// sposition("stream_name")     :: Return the current position of a stream
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
            interpreter.AddStatement("MEMSTREAM", MemStream);
            interpreter.AddStatement("SCOPY", CopyStream);
            interpreter.AddStatement("SREWIND", StreamRewind);

            interpreter.AddFunction("slength", slength);
            interpreter.AddFunction("sposition", sposition);
        }


        private void StreamRewind(IInterpreter interpreter)
        {
            //
            // Syntax SREWIND name
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) implementation 
            var stream = interpreter.GetVar(name);
            checkForStream(interpreter,stream);

            ((Stream)stream.Object).Position=0;

            return;
        }



        private void MemStream(IInterpreter interpreter)
        {
            //
            // Syntax MEMSTREAM name
            //

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            // (v) implementation 
            if (interpreter.IsVariable(name))
            {
                interpreter.Error("STREAMS",Errors.E133_AlreadyDefined(name) );
                return;
            }
            var value = new Value(new MemoryStream(), valueStamp);
            interpreter.SetVar(name, value);

            return;
        }


        private void FileStream(IInterpreter interpreter)
        {
            //
            // Syntax FILESTREAM name, in|inmemory|out, library, path, file_name
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
            file.Library = interpreter.ValueOrVariable().String;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.MatchAny(Token.Identifier, Token.Value);
            file.Path = interpreter.ValueOrVariable().String;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.MatchAny(Token.Identifier, Token.Value);
            file.FileName= interpreter.ValueOrVariable().String;

            interpreter.GetNextToken();

            Value value;
            switch (direction)
            {
                case "IN":
                    var inputStream = interpreter.FileHandler(file).ReadOnlyFileStream();
                    value = new Value(inputStream, valueStamp);
                    break;
                case "INMEMORY":
                    using (var inStream = interpreter.FileHandler(file).ReadOnlyFileStream())
                    {
                        var memStream = new MemoryStream();
                        value = new Value(memStream, valueStamp);
                        inStream.Position = 0;
                        memStream.Position = 0;
                        inStream.CopyTo(memStream);
                        inStream.Flush();
                        inStream.Close();
                        memStream.Position = 0;
                    }
                    break;
                case "OUT":
                    var outputStream = interpreter.FileHandler(file).OutputFileStream(FileShare.ReadWrite);
                    value = new Value(outputStream, valueStamp);
                    break;
                default:
                    interpreter.Error("FILESTREAM", Errors.E106_ExpectingKeyword(direction, "Expecting IN, INMEMORY or OUT"));
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


        private  Value slength(IInterpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("slength", Errors.E125_WrongNumberOfArguments(1)).value;

            var stream = interpreter.GetVar(args[0].String);
            checkForStream(interpreter,stream);


            return new Value(((Stream)stream.Object).Length);
        }

        private Value sposition(IInterpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("sposition", Errors.E125_WrongNumberOfArguments(1)).value;

            var stream = interpreter.GetVar(args[0].String);
            checkForStream(interpreter, stream);


            return new Value(((Stream)stream.Object).Position);
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