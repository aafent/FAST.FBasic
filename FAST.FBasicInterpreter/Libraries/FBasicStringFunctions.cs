namespace FAST.FBasicInterpreter
{
    /*
    String Functions:

    LEN(string): Returns the length (number of characters) of a string.
    LEFT(string, n): Extracts the first n characters from a string.
    RIGHT(string, n): Extracts the last n characters from a string.
    MID(string, start, length): Extracts a substring of a specific length from a string, starting at a given position.
    INSTR(start, string1, string2): Returns the position of the first occurrence of string2 within string1, starting the search at the specified position.
    UCASE(string): Converts a string to uppercase.
    LCASE(string): Converts a string to lowercase.
     */
    public class FBasicStringFunctions : IFBasicLibrary
    {
        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddFunction("len", Len);
            interpreter.AddFunction("left", Left);
            interpreter.AddFunction("right", Right);
            interpreter.AddFunction("mid", Mid);
            interpreter.AddFunction("instr", InStr);
            interpreter.AddFunction("lcase", LCase);
            interpreter.AddFunction("ucase", UCase);
            

        }

        #region (+) FBASIC Functions

        private static Value Len(IInterpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("LEN", Errors.E125_WrongNumberOfArguments(1)).value;
           
            return new Value(args[0].Convert(ValueType.String).String.Length);
        }

        private static Value Left(IInterpreter interpreter, List<Value> args)
        {
            //
            // left(string,size)
            // if the size is 0 returns empty string
            // if the size is negative returns error
            // if the size is greater than the string length returns the whole string
            // if the input string is empty  returns empty string
            //

            string syntax ="left(string,size)";
            if (args.Count !=2 )
                return interpreter.Error("LEFT", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;
            if (string.IsNullOrEmpty(str)) return Value.Empty;


            int size = args[1].ToInt();
            if (size==0) return Value.Empty;
            if (size < 0) 
                return interpreter.Error("LEFT",Errors.E126_WrongArgumentType(1,syntax)).value;

            return new Value(str.Substring(0,size));
        }

        private static Value Right(IInterpreter interpreter, List<Value> args)
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

        private static Value Mid(IInterpreter interpreter, List<Value> args)
        {
            //
            // mid(string,start,length)
            // if the start is less than 1 returns error
            // if the length is less than 1 returns error
            // if the length+start is greater than the string length returns the rest of the string
            // if the input string is empty  returns empty string
            //
            string syntax = "mid(string,start,length)";
            if (args.Count != 3)
                return interpreter.Error("MID", Errors.E125_WrongNumberOfArguments(2, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            int pos = args[1].ToInt();
            if (pos < 1)
                return interpreter.Error("MID", Errors.E126_WrongArgumentType(1, syntax)).value;


            int size = args[2].ToInt();
            if (size < 1)
                return interpreter.Error("MID", Errors.E126_WrongArgumentType(2, syntax)).value;

            if (string.IsNullOrEmpty(str)) return Value.Empty;

            if ( (size + pos) > str.Length )
            {
                return new Value(str.Substring(pos - 1));
            }
            else
            {
                return new Value(str.Substring(pos - 1, size));
            }

            
        }


        private static Value InStr(IInterpreter interpreter, List<Value> args)
        {
            //
            // INSTR(start, string1, string2): Returns the position of the first occurrence of string2 within string1, starting the search at the specified position.
            // if the start is less than 1 returns error    
            // if string1 is empty returns 0
            // if string2 is empty returns start
            // if string2 is not found returns 0
            //
            string syntax = "instr(start,string1,string2)";
            if (args.Count != 3)
                return interpreter.Error("INSTR", Errors.E125_WrongNumberOfArguments(3, syntax)).value;

            int pos = args[0].ToInt();
            if (pos < 1)
                return interpreter.Error("INSTR", Errors.E126_WrongArgumentType(0, syntax)).value;

            string str1 = args[1].Convert(ValueType.String).String;
            string str2 = args[2].Convert(ValueType.String).String;

            if (string.IsNullOrEmpty(str1)) return Value.Zero;
            if (string.IsNullOrEmpty(str2)) return args[0];

            return new Value(str1.IndexOf(str2,pos) + 1);
        }

        private static Value UCase(IInterpreter interpreter, List<Value> args)
        {
            string syntax = "UCASE(string)";
            if (args.Count != 1)
                return interpreter.Error("UCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            return new Value(str.ToUpper());
        }


        private static Value LCase(IInterpreter interpreter, List<Value> args)
        {
            string syntax = "LCASE(string)";
            if (args.Count != 1)
                return interpreter.Error("LCASE", Errors.E125_WrongNumberOfArguments(1, syntax)).value;

            string str = args[0].Convert(ValueType.String).String;

            return new Value(str.ToLower());
        }

        #endregion (+) FBASIC Functions

    }

}
