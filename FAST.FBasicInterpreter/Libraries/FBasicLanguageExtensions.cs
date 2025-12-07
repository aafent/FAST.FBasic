//
// Language Extensions Statements: 
//
// 

using System.Text;

namespace FAST.FBasicInterpreter
{
    public class FBasicLanguageExtensions : IFBasicLibraryWithMemory, IDisposable
    {
        private IInterpreter inter;
        private Dictionary<string,int> arguments = new();

        public string uniqueName => "FBasicLanguageExtensions";

        public FBasicLanguageExtensions()
        {
            //IFBasicFileManagementLayer fileManager=new zzzzzzz();
            //this.fileManager=fileManager;
        }

        public void InstallAll(IInterpreter interpreter)
        {
            this.inter = interpreter;

            interpreter.AddStatement("BLOCK", BLOCK);
            interpreter.AddStatement("STATEMENT", STATEMENT);

        }



        private void STATEMENT(IInterpreter interpreter)
        {
            // Syntax: STATEMENT num_of_arguments, name_of_statement
            //  
            interpreter.Match(Token.Value);
            int argno = interpreter.lex.Value.ToInt();
            interpreter.GetNextToken();

            interpreter.Match(Token.Comma);
            interpreter.GetNextToken();

            interpreter.Match(Token.Identifier);
            string name = interpreter.lex.Identifier;
            interpreter.GetNextToken();

            var code = interpreter.GetBlockCode("ENDSTATEMENT");

            // (v) implementation
            var nameUpper = name.ToUpper();
            if (arguments.ContainsKey(nameUpper))
            {
                interpreter.Error("L-EXT",Errors.E133_AlreadyDefined("Statement",name));
                return;
            }
            arguments.Add(nameUpper,argno);

            var statementVarName="$ST$_"+name.ToUpper();
            interpreter.SetVar(statementVarName, new Value(code));

            interpreter.AddStatement(name, UDStatement);
            interpreter.RefreshLexerStatements();

            return;
        }

        private void UDStatement(IInterpreter interpreter)
        {
            var name= interpreter.lex.AddOn.ToUpper();
            if (!arguments.ContainsKey(name))
            {
                interpreter.Error("L-EXT", Errors.E132_InternalError($"Cannot retrieve UDS {name} metadata"));
                return;
            }
            var argNo = arguments[name];

            List<Object> argValues = new();
            for(int times=0; times<argNo; times++)
            {
                if (times > 0)
                {
                    interpreter.Match(Token.Comma);
                    interpreter.GetNextToken();
                }

                var value = interpreter.ValueOrVariable(true);
                switch(value.Type)
                {
                    case ValueType.String:
                        argValues.Add(value.String);
                        break;
                    case ValueType.Real:
                        argValues.Add(value.Real);
                        break;
                    case ValueType.Object:
                        argValues.Add(value.Object);
                        break;
                }
                interpreter.GetNextToken();
            }

            ArgumentsToInput args = new(argValues.ToArray());
            var code=interpreter.GetVar("$ST$_"+name).String;
            if (!string.IsNullOrWhiteSpace(code))
            {
                var oldHandler = interpreter.InputHandler;
                interpreter.InputHandler= () => args.GetNextArgumentAsInput();
                interpreter.Eval(code,true);
                interpreter.InputHandler=oldHandler;
            }
        }


        private void BLOCK(IInterpreter interpreter)
        {
            // Syntax: BLOCK variable_to_save
            //  
            interpreter.Match(Token.Identifier);
            string name=interpreter.lex.Identifier;
            interpreter.GetNextToken();

            var code=interpreter.GetBlockCode("ENDBLOCK");
            interpreter.SetVar(name,new Value(code));

            return;
        }


        public void ClearMemory()
        {
        }

        public void Dispose()
        {
            foreach (var statement in arguments)
            {
                inter.RemoveStatement(statement.Key);
            }
            inter.RefreshLexerStatements();
            this.arguments.Clear();
            ClearMemory();
        }
    }

}