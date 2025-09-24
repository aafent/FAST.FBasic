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

        public static Value Str(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return args[0].Convert(ValueType.String);
        }

        public static Value Num(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                throw new ArgumentException();

            return args[0].Convert(ValueType.Real);
        }

        public static Value Abs(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("SCI", Errors.E125_WrongNumberOfArguments(1)).value;

            return new Value(Math.Abs(args[0].Real));
        }

        public static Value Min(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 2)
                return interpreter.Error("SCI", Errors.E125_WrongNumberOfArguments(2)).value;

            return new Value(Math.Min(args[0].Real, args[1].Real));
        }

        public static Value Max(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("SCI", Errors.E125_WrongNumberOfArguments(1)).value;

            return new Value(Math.Max(args[0].Real, args[1].Real));
        }

        public static Value Not(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("SCI", Errors.E125_WrongNumberOfArguments(1)).value;

            return new Value(args[0].Real == 0 ? 1 : 0);
        }

        public static void LogInfo(Interpreter interpreter)
        {
            interpreter.log.info(interpreter.Expr().ToString());
        }

    }

}
