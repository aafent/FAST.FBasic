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

        public string uniqueName => "FBasicLanguageExtensions";

        public FBasicLanguageExtensions()
        {
            //IFBasicFileManagementLayer fileManager=new zzzzzzz();
            //this.fileManager=fileManager;
        }

        public void InstallAll(IInterpreter interpreter)
        {
            this.inter = interpreter;

            interpreter.AddStatement("EVAL", EVAL);

        }

        private static void EVAL(IInterpreter interpreter)
        {
            // Syntax: EVAL value|variable
            //  
            string code = interpreter.ValueOrVariable(true).String;
            interpreter.GetNextToken();


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