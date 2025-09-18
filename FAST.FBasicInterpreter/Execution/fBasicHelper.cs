namespace FAST.FBasicInterpreter
{
    public class fBasicHelper
    {
        public static string toSource(programContainer program)
        {
            IsourceCodeBuilder builder = new fBasicSourceBuilder();
            builder.Build(program);
            return builder.GetSource();
        }

        public static executionResult runSource(executionEnvironment env, string sourceCode, Action<Interpreter> action =null )
        {
            Interpreter basic = new Interpreter(true, sourceCode);
            if (env.printHandler != null) basic.printHandler = env.printHandler;
            if (env.inputHandler != null) basic.inputHandler = env.inputHandler;
            if (env.callHandler != null) basic.callHandler = env.callHandler;
            if (env.requestForObject != null) basic.requestForObjectHandler = env.requestForObject;
            if ( action!=null) action(basic);
            return basic.ExecWithResult();
        }
        public static executionResult run(executionEnvironment env, string fileName, Action<Interpreter> action = null)
        {
            var src = File.ReadAllText(fileName);
            return runSource(env,src,action);
        }
        public static executionResult run(executionEnvironment env, programContainer program, Action<Interpreter> action = null) 
        {
            var src = fBasicHelper.toSource(program);
            return runSource(env,src,action);
        }

        public static programContainer toProgram(string fileName)
        {
            var src = File.ReadAllText(fileName);
            Interpreter basic = new Interpreter(true, src);
            programContainer program;
            if ( basic.tryParseSourceCode(out program) ) return program;
            return null;
        }

    }
}
