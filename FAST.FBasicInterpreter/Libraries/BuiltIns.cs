namespace FAST.FBasicInterpreter
{
    internal class BuiltIns : IFBasicLibrary
    {
        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("str", Str);
            interpreter.AddFunction("num", Num);
            interpreter.AddFunction("abs", Abs);
            interpreter.AddFunction("min", Min);
            interpreter.AddFunction("max", Max);
            interpreter.AddFunction("not", Not);

            interpreter.AddStatement("LogInfo",LogInfo);
        }

        private static Value Str(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("str",Errors.E125_WrongNumberOfArguments(1)).value;

            return args[0].Convert(ValueType.String);
        }

        private static Value Num(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("num", Errors.E125_WrongNumberOfArguments(1)).value;

            return args[0].Convert(ValueType.Real);
        }

        private static Value Abs(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("abs", Errors.E125_WrongNumberOfArguments(1)).value;

            return new Value(Math.Abs(args[0].Real));
        }

        private static Value Min(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                return interpreter.Error("min", Errors.E125_WrongNumberOfArguments(2)).value;

            return new Value(Math.Min(args[0].Real, args[1].Real));
        }

        private static Value Max(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("max", Errors.E125_WrongNumberOfArguments(2)).value;

            return new Value(Math.Max(args[0].Real, args[1].Real));
        }

        private static Value Not(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("Not", Errors.E125_WrongNumberOfArguments(1)).value;

            return args[0].Real == Value.FalseValue ? Value.True: Value.False;
        }

        private static void LogInfo(Interpreter interpreter)
        {
            interpreter.log.info(interpreter.Expr().ToString());
        }

    }

}
