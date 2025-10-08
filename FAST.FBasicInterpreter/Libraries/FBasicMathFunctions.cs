namespace FAST.FBasicInterpreter
{
    /*
    Math Functions:

    BASIC,  includes a variety of built-in mathematical functions to perform common calculations. Here's a list of some of the most common ones.

    Trigonometric Functions
    SIN(X): Returns the sine of X. The value of X must be in radians.
    COS(X): Returns the cosine of X. The value of X must be in radians.
    TAN(X): Returns the tangent of X. The value of X must be in radians.
    ATN(X): Returns the arctangent of X. The result is in radians.

    Logarithmic and Exponential Functions
    LOG(X): Returns the natural logarithm (base e) of X.
    EXP(X): Returns the value of e raised to the power of X. This is the inverse of the LOG function.

    Absolute Value and Sign
    ABS(X): Returns the absolute value of X. This function is useful for ensuring a number is non-negative.
    SGN(X): Returns the sign of X. It returns 1 if X>0, -1 if X<0, and 0 if X=0.

    Integer and Rounding Functions
    INT(X): Returns the largest integer less than or equal to X. For positive numbers, this is equivalent to truncating the decimal part.
    FIX(X): Returns the integer part of X by truncating the decimal part. For negative numbers, it behaves differently from INT. For example, FIX(-3.7) returns -3, while INT(-3.7) returns -4.
    ROUND(X,D): Rounds X to D decimal places. 

    Powers
    SQR(X): Square Root. Returns the square root of X. The value of X must be non-negative.

    Other
    RND(): This function is used to generate a random number. Its behavior can vary between different versions of BASIC. Often, RND without an argument generates a random number between 0 and 1.
    MOD(X, Y): Returns the remainder of X divided by Y. This is useful for various calculations, including determining even or odd numbers.

    PI: A constant representing the value of π (approximately 3.14159). This is not a function but a predefined constant in many BASIC dialects.

     */
    public class FBasicMathFunctions : IFBasicLibrary
    {
        public void InstallAll(Interpreter interpreter)
        {
            // (v) Trigonometric
            interpreter.AddFunction("sin", Sin);
            interpreter.AddFunction("cos", Cos);
            interpreter.AddFunction("tan", Tan);
            interpreter.AddFunction("atn", Atn);

            // (v) Logarithmic
            interpreter.AddFunction("log", Log);
            interpreter.AddFunction("exp", Exp);

            // (v) Rounding
            interpreter.AddFunction("int", IntFunction);
            interpreter.AddFunction("fix", Fix);
            interpreter.AddFunction("round", (inter, args) => DoMath2("ROUND", inter, args));

            // (v) Other Math
            interpreter.AddFunction("sqr", Sqr);
            interpreter.AddFunction("sgn", Sgn);
            interpreter.AddFunction("rnd", Rnd);
            interpreter.AddFunction("mod", (inter,args)=> DoMath2("MOD", inter, args));


            // (v) Constants
            interpreter.SetVar("PI",new Value(Math.PI));
        }



        private static Value DoMath2(string function, Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 2)
                return interpreter.Error(function, Errors.E125_WrongNumberOfArguments(2)).value;
            var arg1 = args[0].Convert(ValueType.Real).Real;
            var arg2 = args[1].Convert(ValueType.Real).Real;
            switch (function)
            {
                // (v) Rounding 
                case "ROUND":
                    return new Value(Math.Round(arg1,(int)arg2));

                // (v) Other
                case "MOD":
                    return new Value(arg1 % arg2); // int remainder = dividend % divisor; // remainder will be 1

            }
            return Value.Error;
        }


        private static Value DoMath(string function, Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 1)
                return interpreter.Error(function, Errors.E125_WrongNumberOfArguments(1)).value;
            var arg1 = args[0].Convert(ValueType.Real).Real;
            switch(function)
            {
                // (v) Trigonometric 
                case "COS":
                    return new Value(Math.Cos(arg1));
                case "SIN":
                    return new Value(Math.Sin(arg1));
                case "TAN":
                    return new Value(Math.Tan(arg1));
                case "ATN":
                    return new Value(Math.Atan(arg1));

                // (v) Logarithmic 
                case "LOG":
                    return new Value(Math.Log(arg1));
                case "EXP":
                    return new Value(Math.Exp(arg1));

                // (v) Other Math
                case "SGN":
                    return new Value(Math.Sign(arg1));
                case "SQR":
                    return new Value(Math.Sqrt(arg1));

                // (v) Rounding 
                case "INT":
                    return new Value(Math.Floor(arg1));
                case "FIX":
                    return new Value(Math.Truncate(arg1));


            }
            return Value.Error;
        }


        public static Value Rnd(Interpreter interpreter, List<Value> args)
        {
            if (args.Count != 0)
                return interpreter.Error("RND", Errors.E125_WrongNumberOfArguments(0)).value;
            return new Value(new Random().NextDouble());
        }


        public static Value Round(Interpreter interpreter, List<Value> args)
            => DoMath2("ROUND", interpreter, args);

        public static Value Modulo(Interpreter interpreter, List<Value> args)
            => DoMath2("MOD", interpreter, args);

        public static Value Cos(Interpreter interpreter, List<Value> args) 
            => DoMath("COS",interpreter, args);

        public static Value Sin(Interpreter interpreter, List<Value> args)
            => DoMath("SIN", interpreter, args);

        public static Value Tan(Interpreter interpreter, List<Value> args)
            => DoMath("TAN", interpreter, args);

        public static Value Atn(Interpreter interpreter, List<Value> args)
            => DoMath("ATN", interpreter, args);


        public static Value Log(Interpreter interpreter, List<Value> args)
            => DoMath("LOG", interpreter, args);

        public static Value Exp(Interpreter interpreter, List<Value> args)
            => DoMath("EXP", interpreter, args);


        public static Value Sgn(Interpreter interpreter, List<Value> args)
            => DoMath("SGN", interpreter, args);


        public static Value Sqr(Interpreter interpreter, List<Value> args)
            => DoMath("SQR", interpreter, args);


        public static Value IntFunction(Interpreter interpreter, List<Value> args)
            => DoMath("INT", interpreter, args);

        public static Value Fix(Interpreter interpreter, List<Value> args)
            => DoMath("FIX", interpreter, args);
    }

}
