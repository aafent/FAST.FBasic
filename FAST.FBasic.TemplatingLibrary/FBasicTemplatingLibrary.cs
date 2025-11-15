// package: DocumentFormat.OpenXml
// Stream Statements: 
//
// WORDEXTRACTBODY input_stream_name, identifier 
// WORDREPALCEBODY stream, ALL|FIRST, array
// WORDAPPEND stream_to_append, destination_stream
// WORDEMPTYDOC stream
// WORDPAGEBREAK stream
// WORDAPPENDROW stream, marker
// WORDDELETEROW stream, marker
// WORDEXTRACTPART input_stream_name, output_stream_name, begin_marker, end_market 
// WORDDELETEPART input_stream_name, begin_marker, end_market 
//
// todo: WORDFROMMARKDOWN variable, output_word_stream 
// 
// todo: WORDREPALCEPART input_stream_name, input_part_name, marker

using FAST.FBasicInterpreter;

namespace FAST.FBasic.TemplatingLibrary
{
    public class FBasicTemplatingLibrary  : IFBasicLibraryWithMemory
    {
        private const string valueStamp = "_$$Stream$$_";
        private const string markerPrefix = "[<";
        private const string markerSuffix = ">]";
        private const string placeHolderPrefix = "{";
        private const string placeHolderSuffix = "}";
        private IInterpreter inter;

        public string uniqueName => "TemplatingLibrary";

        public FBasicTemplatingLibrary()
        {
        }


        public void InstallAll(IInterpreter interpreter)
        {
            this.inter = interpreter;
            interpreter.AddStatement("WORDEXTRACTBODY", WordExtractBody);
            interpreter.AddStatement("WORDREPALCEBODY", WordReplaceBody);
            interpreter.AddStatement("WORDAPPEND", WordMergeDocuments);
            interpreter.AddStatement("WORDPAGEBREAK", WordPageBreak);
            
            interpreter.AddStatement("WORDEMPTYDOC", WordEmptyDocument);
            interpreter.AddStatement("WORDAPPENDROW", WordAppendRow);
            interpreter.AddStatement("WORDDELETEROW", WordDeleteRow);

            interpreter.AddStatement("WORDEXTRACTPART", WordExtractPart);
            interpreter.AddStatement("WORDDELETEPART", WordDeletePart);

            interpreter.AddStatement("WORDFROMMARKDOWN", WordFromMarkDown);

        }


        private void WordFromMarkDown(IInterpreter interpreter)
        {
            // Syntax: WORDFROMMARKDOWN variable|value, output_word_stream 

            // (v) argument: variable
            var mdtext = interpreter.ValueOrVariable(true);
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var name=interpreter.lex.Identifier;
            
            interpreter.GetNextToken();

            Stream stream;
            if (interpreter.IsVariable(name) )
            {
                var streamVar = interpreter.GetVar(name);
                checkForStream(interpreter, streamVar);
                stream=((Stream)streamVar.Object);
            }
            else
            {
                stream = new WordPlaceHolder().CreateEmptyWordDocument();
                interpreter.SetVar(name,new Value(stream,valueStamp));
            }

            var wordStream=MarkdownToWordConverter.ConvertMarkdownToWord(mdtext.ToString());
            File.WriteAllBytes(@"c:\temp\output.docx", ((MemoryStream)wordStream).ToArray());
            wordStream.Position=0;
            new WordDocumentMerger().AppendDocument(stream, wordStream);
        }



        private void WordAppendRow(IInterpreter interpreter)
        {
            // Syntax: WORDAPPENDROW stream, marker

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var marker = interpreter.ValueOrVariable(doMatch: true).String;
            interpreter.GetNextToken();

            // (v) implementation 
            var streamVar = interpreter.GetVar(name);
            checkForStream(interpreter, streamVar);

            new WordMarker(markerPrefix, markerSuffix).DuplicateRowWithMarker(((Stream)streamVar.Object), marker, out bool found);
            interpreter.SetVar("FOUND",new Value(found));
        }

        private void WordDeleteRow(IInterpreter interpreter)
        {
            // WORDDELETEROW stream, marker

            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var marker = interpreter.ValueOrVariable(doMatch: true).String;
            interpreter.GetNextToken();

            // (v) implementation 
            var streamVar = interpreter.GetVar(name);
            checkForStream(interpreter, streamVar);

            new WordMarker(markerPrefix,markerSuffix).RemoveRowsByMarker(((Stream)streamVar.Object), marker, out bool found);
            interpreter.SetVar("FOUND", new Value(found));
        }


        private void WordDeletePart(IInterpreter interpreter)
        {
            //
            // Syntax: WORDDELETEPART input_stream_name, begin_marker, end_market 
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string inputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var beginMarker = interpreter.ValueOrVariable(doMatch: true);
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var endMarker = interpreter.ValueOrVariable(doMatch: true);
            interpreter.GetNextToken();

            // (v) implement 
            var inputStreamVar = interpreter.GetVar(inputName);
            checkForStream(interpreter, inputStreamVar);

            new WordMarker(markerPrefix, markerSuffix).RemoveContentBetweenMarkers(
                                    ((Stream)inputStreamVar.Object),
                                    beginMarker.String,
                                    endMarker.String,
                                    out bool found
                                );
            interpreter.SetVar("FOUND", new Value(found));
        }



        private void WordExtractPart(IInterpreter interpreter)
        {
            //
            // Syntax: WORDEXTRACTPART input_stream_name, output_stream_name, begin_marker, end_market 
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string inputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var outputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var beginMarker = interpreter.ValueOrVariable(doMatch: true);
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            var endMarker = interpreter.ValueOrVariable(doMatch: true);
            interpreter.GetNextToken();

            // (v) implement 
            var inputStreamVar = interpreter.GetVar(inputName);
            checkForStream(interpreter, inputStreamVar);

            var outStream = new MemoryStream();
            new WordMarker(markerPrefix,markerSuffix).ExtractContentBetweenMarkers(
                                    ((Stream)inputStreamVar.Object),
                                    outStream,
                                    beginMarker.String,
                                    endMarker.String,
                                    out bool found
                                );
            interpreter.SetVar("FOUND",new Value(found));
            if (found) interpreter.SetVar(outputName, new Value(outStream, valueStamp)  );

        }


        private void WordEmptyDocument(IInterpreter interpreter)
        {
            // WORDEMPTYDOC stream
            //
            // (v) argument: name
            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            var stream = new WordPlaceHolder().CreateEmptyWordDocument();
            
            interpreter.SetVar(name,new Value(stream,valueStamp ) );
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
            new WordPlaceHolder().AddPageBreak(append);

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

            var text= new WordPlaceHolder().ExtractAllText(((Stream)stream.Object));

            interpreter.SetVar(variableToSave,new Value(text) );
        }

        private void WordReplaceBody(IInterpreter interpreter)
        {
            //
            // Syntax: REPALCEWORDBODY word_stream, array
            //
            // (v) argument: word_stream
            interpreter.Match(Token.Identifier);
            string inputName = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            interpreter.MatchAndThenNextToken(Token.Comma);

            interpreter.Match(Token.Identifier);
            var policy = interpreter.lex.Identifier.ToUpper();
            switch (policy)
            {
                case "ALL":
                case "FIRST":
                    break;
                default:
                    interpreter.Error(Errors.E106_ExpectingKeyword(policy),"Expecting ALL or FIRST" );
                    return;
            }
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
                Console.WriteLine(item.Value);
                replacements[item.Key] = ToolKitHelper.FormatStringValue(item.Key, interpreter.GetValue(item.Value).ToString());
            }
            new WordPlaceHolder(placeHolderPrefix,placeHolderSuffix).ReplacePlaceholders(((Stream)stream.Object), replacements, policy);

            return;
        }




        private void checkForStream(IInterpreter interpreter, Value value)
        {
            if (value.Type != FBasicInterpreter.ValueType.Object)
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