namespace FAST.FBasicInterpreter
{
    /*
    Date functions in the BASIC programming language are used to handle and manipulate dates and times. These functions allow programs to retrieve the current date and time, perform calculations with dates, and format dates for display.

    Common Date and Time Functions
    DATE(): This function returns a string representing the current system date. The format typically includes the month, day, and year, such as MM-DD-YYYY or DD-MM-YYYY.
    ISDATE(d): Check if the input string is a valid date.

    JTOD(): Converts the japan style of date (YYYY-MM-DD) to International (DD-MM-YYYY)
    DTOJ(): Converts the International (DD-MM-YYYY) date to japan style of date (YYYY-MM-DD) 

    DAY(), MONTH(), YEAR(): These functions extract the day, month, or year as an integer from a date variable. For example, MONTH("10-26-2025") would return 10.

    TIME(): This function returns a string representing the current system time. The format is usually HH:MM:SS.

    HOUR(), MINUTE(), SECOND(): These functions extract the hour, minute, or second as an integer from a time variable.
     */
    public class FBasicDateFunctions : IFBasicLibrary
    {
        public const string dateFormat = "dd-MM-yyy";
        public const string timeFormat = "HH:mm:ss.fff"; 

        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddFunction("date", DateFunction);
            interpreter.AddFunction("isdate", IsDate);
            interpreter.AddFunction("jtod", JtoD);
            interpreter.AddFunction("dtoj", DtoJ);
            interpreter.AddFunction("day", Day);
            interpreter.AddFunction("month", Month);
            interpreter.AddFunction("year", Year);

            interpreter.AddFunction("time", TimeFunction);
            interpreter.AddFunction("hour", Hour);
            interpreter.AddFunction("minute", Minute);
            interpreter.AddFunction("second", Second);
        }

        public static Value DateFunction(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 0)
                return interpreter.Error("DATE", Errors.E125_WrongNumberOfArguments(0)).value;
 
            return new Value(DateTime.Now.ToString(dateFormat));
        }
        public static Value JtoD(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("JTOD", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if ( value.Length!=10 )
                return interpreter.Error("JTOD", Errors.E127_WrongArgumentReferredType("DATE")).value;

            //YYYY-MM-DD --> DD-MM-YYYY
            value = value.Substring(8, 2) + "-" +
                    value.Substring(5, 2) + "-" +
                    value.Substring(0, 4);

            if (!isValidDFormat(value))
                return interpreter.Error("JTOD", Errors.E126_WrongArgumentType(1)).value;

            return new Value(value);
        }
        public static Value DtoJ(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("DTOJ", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (value.Length != 10)
                return interpreter.Error("DTOJ", Errors.E127_WrongArgumentReferredType("DATE")).value;

            //DD-MM-YYYY --> YYYY-MM-DD
            value = value.Substring(6, 4) + "-" +
                    value.Substring(3, 2) + "-" + 
                    value.Substring(0, 2);

            return new Value(value);
        }

        public static Value IsDate(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("ISDATE", Errors.E125_WrongNumberOfArguments(1)).value;

            if (isValidDFormat(args[0])) return Value.True;

            return Value.False;
        }
     
        public static Value Day(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("DAY", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (value.Length != 10)
                return interpreter.Error("DAY", Errors.E127_WrongArgumentReferredType("DATE")).value;

            //DD-MM-YYYY 
            value = value.Substring(0, 2);

            return new Value(int.Parse(value) );
        }
        public static Value Month(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("MONTH", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (value.Length != 10)
                return interpreter.Error("MONTH", Errors.E127_WrongArgumentReferredType("DATE")).value;

            //DD-MM-YYYY 
            value = value.Substring(3, 2);

            return new Value(int.Parse(value));
        }
        public static Value Year(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("YEAR", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (value.Length != 10)
                return interpreter.Error("YEAR", Errors.E127_WrongArgumentReferredType("DATE")).value;

            //DD-MM-YYYY 
            value = value.Substring(6, 4);

            return new Value(int.Parse(value));
        }

        public static Value TimeFunction(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 0)
                return interpreter.Error("TIME", Errors.E125_WrongNumberOfArguments(0)).value;

            return new Value(DateTime.Now.ToString(timeFormat));
        }

        public static Value Hour(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("HOUR", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (!( value.Length != 5 || value.Length != 8 || (value.Length > 9 && value.Length<13) ))
                return interpreter.Error("HOUR", Errors.E127_WrongArgumentReferredType("TIME")).value;
            
            //HH:mm:ss.fff or HH:mm or HH:mm:ss or HH:mm:ss.f or HH:mm:ss.ff or HH:mm:ss.fff
            value = value.Substring(0, 2);

            return new Value(int.Parse(value));
        }
        public static Value Minute(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("MINUTE", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (!(value.Length != 5 || value.Length != 8 || (value.Length > 9 && value.Length < 13)))
                return interpreter.Error("MINUTE", Errors.E127_WrongArgumentReferredType("TIME")).value;

            //HH:mm:ss.fff or HH:mm or HH:mm:ss or HH:mm:ss.f or HH:mm:ss.ff or HH:mm:ss.fff
            value = value.Substring(3, 2);

            return new Value(int.Parse(value));
        }
        public static Value Second(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error("SECOND", Errors.E125_WrongNumberOfArguments(1)).value;

            string value = args[0].String;
            if (!(value.Length != 5 || value.Length != 8 || (value.Length > 9 && value.Length < 13)))
                return interpreter.Error("SECOND", Errors.E127_WrongArgumentReferredType("TIME")).value;

            //HH:mm:ss.fff or HH:mm or HH:mm:ss or HH:mm:ss.f or HH:mm:ss.ff or HH:mm:ss.fff
            value = value.Substring(6, 2);

            return new Value(int.Parse(value));
        }



        private static bool isValidDFormat(Value value) => isValidDFormat(value.String);
        private static bool isValidDFormat(string value)
        {
            if (value.Length != 10) return false;
            if (!(value[2] == '-' && value[5] == '-') ) return false;
            // normally we have to check for numeric digits .....
            return true;   
        }

    }

}
