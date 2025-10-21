namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// FBasic source program helper
    /// </summary>
    public class FBasicSource
    {
        /// <summary>
        /// Program container to source
        /// </summary>
        /// <param name="program">The program container</param>
        /// <returns>string, the source program</returns>
        public static string ToSource(ProgramContainer program)
        {
            IsourceCodeBuilder builder = new fBasicSourceBuilder();
            builder.Build(program);
            return builder.GetSource();
        }

        /// <summary>
        /// Run a FBasic program
        /// </summary>
        /// <param name="env">The running environment</param>
        /// <param name="sourceCode">The source code to run</param>
        /// <param name="action">Optional, Interpreter actions before run</param>
        /// <returns>executionResult</returns>
        public static ExecutionResult RunSource(ExecutionEnvironment env, string sourceCode, Action<Interpreter> action =null )
        {
            if (env==null) // in case the no environment, set the default environment. 
            {
                env=new();
                env.DefaultEnvironment();
            }
            Interpreter basic = new Interpreter(env.installBuiltIns, sourceCode);
            env.SetupInterpreter(basic);
            if ( action!=null) action(basic);

            return basic.ExecWithResult();
        }

        /// <summary>
        /// Run a FBasic program
        /// </summary>
        /// <param name="env">The running environment</param>
        /// <param name="fileName">Full path to file with the source program</param>
        /// <param name="action">Optional, Interpreter actions before run</param>
        /// <returns>executionResult</returns>
        public static ExecutionResult Run(ExecutionEnvironment env, string fileName, Action<Interpreter> action = null)
        {
            var src = File.ReadAllText(fileName);
            return RunSource(env,src,action);
        }

        /// <summary>
        /// Run a FBasic program (in container)
        /// </summary>
        /// <param name="env">The running environment</param>
        /// <param name="program">The program container</param>
        /// <param name="action">Optional, Interpreter actions before run</param>
        /// <returns>executionResult</returns>
        public static ExecutionResult Run(ExecutionEnvironment env, ProgramContainer program, Action<Interpreter> action = null) 
        {
            var src = ToSource(program);
            return RunSource(env,src,action);
        }

        /// <summary>
        /// Convert file program to program container
        /// </summary>
        /// <param name="fileName">File path to the source file name</param>
        /// <returns>programContainer</returns>
        public static ProgramContainer ToProgram(string fileName)
        {
            var src = File.ReadAllText(fileName);
            Interpreter basic = new Interpreter(true, src);
            ProgramContainer program;
            if ( basic.TryParseSourceCode(out program) ) return program;
            return null;
        }
    }
}
