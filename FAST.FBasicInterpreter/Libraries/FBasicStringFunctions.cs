namespace FAST.FBasicInterpreter
{
/*
String Functions:

LEN(string): Returns the length (number of characters) of a string.
LEFT(string, n): Extracts the first n characters from a string.
RIGHT(string, n): Extracts the last n characters from a string.
MID(string, start, length): Extracts a substring of a specific length from a string, starting at a given position.
UCASE(string): Converts a string to uppercase.
LCASE(string): Converts a string to lowercase.
 */
    public class FBasicStringFunctions : IFBasicLibrary
    {
        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("len", Len);
            interpreter.AddFunction("left", Left);
            interpreter.AddFunction("right", Right);
            interpreter.AddFunction("mid", Mid);
            interpreter.AddFunction("lcase", LCase);
            interpreter.AddFunction("ucase", UCase);
        }

        public static Value Len(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("LEN", Errors.E125_WrongNumberOfArguments(1)).value;
           
            return new Value(args[0].Convert(ValueType.String).String.Length);
        }

        public static Value Left(Interpreter interpreter, List<Value> args)
        {
            string syntax="LEFT(string,size)";
            if (args.Count !=2 )
                return interpreter.Error("LEFT", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            int size = args[1].ToInt();
            if (size < 1) 
                return interpreter.Error("LEFT",Errors.E126_WrongArgumentType(1,syntax)).value;

            return new Value(str.Substring(0,size));
        }

        public static Value Right(Interpreter interpreter, List<Value> args)
        {
            string syntax = "RIGHT(string,size)";
            if (args.Count != 2)
                return interpreter.Error("RIGHT", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            int size = args[1].ToInt();
            if (size < 1)
                return interpreter.Error("RIGHT", Errors.E126_WrongArgumentType(1, syntax)).value;

            if ((str.Length - size)<str.Length) 
                return new Value(str);

            return new Value(str.Substring(str.Length - size));
        }

        public static Value Mid(Interpreter interpreter, List<Value> args)
        {
            string syntax = "MID(string,start,length)";
            if (args.Count != 3)
                return interpreter.Error("MID", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            int pos = args[1].ToInt();
            if (pos < 1)
                return interpreter.Error("MID", Errors.E126_WrongArgumentType(1, syntax)).value;


            int size = args[2].ToInt();
            if (size < 1)
                return interpreter.Error("MID", Errors.E126_WrongArgumentType(2, syntax)).value;

            return new Value(str.Substring(pos - 1, size));
        }



        public static Value UCase(Interpreter interpreter, List<Value> args)
        {
            string syntax = "UCASE(string)";
            if (args.Count != 1)
                return interpreter.Error("UCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            return new Value(str.ToUpper());
        }


        public static Value LCase(Interpreter interpreter, List<Value> args)
        {
            string syntax = "LCASE(string)";
            if (args.Count != 1)
                return interpreter.Error("LCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            return new Value(str.ToLower());
        }

    }

}
