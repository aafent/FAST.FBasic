using FAST.FBasicInterpreter.DataProviders;

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
            interpreter.AddFunction("empty", Empty);
            interpreter.AddFunction("ubound", UpperBound);

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

        private static Value Empty(Interpreter interpreter, List<Value> args)
        {
            if (args.Count < 1)
                return interpreter.Error("empty", Errors.E125_WrongNumberOfArguments(1)).value;

            var value = args[0];

            switch (value.Type)
            { 
                case ValueType.String:
                    return string.IsNullOrEmpty(value.String)?Value.True : Value.False;
                case ValueType.Real:
                    return value.Real == 0 ? Value.True : Value.False;
                case ValueType.Object:
                    return value.Object == null ? Value.True : Value.False;
                default:
                    throw new NotImplementedException($"Empty() not implemented for type {value.Type.ToString()}");
            }



            return args[0].Real == Value.FalseValue ? Value.True : Value.False;
        }

        private static void LogInfo(Interpreter interpreter)
        {
            interpreter.logger.info(interpreter.Expr().ToString());
        }

        private Value UpperBound(Interpreter interpreter, List<Value> args)
        {
            string syntax = "ubound(array_or_collection_name)";
            if (args.Count != 1)
                return interpreter.Error("ubound", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            var name = args[0].String;

            if (interpreter.IsArray(name))
            {
                FBasicArray array = interpreter.GetArray(name);
                return new Value(array.Length);
            }
            if (interpreter.IsCollection(name))
            {
                var collection = interpreter.collections[name];
                if (!(collection is staticDataCollection))
                {
                    return interpreter.Error("ubound", Errors.E127_WrongArgumentReferredType("SDATA Collection")).value;
                }
                var count = ((staticDataCollection)interpreter.collections[name]).data.Count;
                return new Value(count);
            }

            return interpreter.Error("ubound", Errors.E127_WrongArgumentReferredType("expecting Array or collection")).value;

        }


    }

}
