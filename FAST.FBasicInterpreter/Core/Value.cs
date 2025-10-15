using System.Reflection.Metadata;

namespace FAST.FBasicInterpreter
{

    /// <summary>
    /// Value type
    /// </summary>
    public enum ValueType
    {
        Real, // it's double
        String,
        Object
    }

    public struct Value : IComparable<Value>
    {

        /// <summary>
        /// The date format used in the interpreter
        /// </summary>
        public const string dateFormat = "dd-MM-yyy";

        /// <summary>
        /// The time format used in the interpreter
        /// </summary>
        public const string timeFormat = "HH:mm:ss.fff";

        /// <summary>
        /// The numeric value of True
        /// </summary>
        public const int TrueValue = -1;

        /// <summary>
        /// The numeric value of False
        /// </summary>
        public const int FalseValue=0;

        /// <summary>
        /// The value used as Zero
        /// </summary>
        public static readonly Value Zero = new Value(0);

        /// <summary>
        /// The value used as Empty string
        /// </summary>
        public static readonly Value Empty = new Value(string.Empty);

        public static readonly Value Wildcard = new Value("*"){ Real= FalseValue };

        /// <summary>
        /// Value for the boolean true
        /// </summary>
        public static readonly Value True = new Value(TrueValue);

        /// <summary>
        /// Value for the boolean false
        /// </summary>
        public static readonly Value False  = new Value(FalseValue);


        /// <summary>
        /// Used to return an Error. 
        /// Any Error casted to 0 value
        /// </summary>
        public static readonly Value Error = new Value(0);

        /// <summary>
        /// The type of the value
        /// </summary>
        public ValueType Type { get; set; }

        public double Real { get; set; }
        public string String { get; set; }
        public object Object { get; set; }

        /// <summary>
        /// Constructor with a real as initial value
        /// </summary>
        /// <param name="real">the real number</param>
        public Value(double real) : this()
        {
            this.Type = ValueType.Real;
            this.Real = real;
        }

        /// <summary>
        /// Constructor with a value an integer casted to Real as initial value
        /// </summary>
        /// <param name="value">The integer</param>
        public Value(int value)
        {
            this.Type = ValueType.Real;
            this.Real = value;
        }

        /// <summary>
        /// Constructor with a boolean as initial value that it is casted to the appropriate Real. 
        /// </summary>
        /// <param name="value">the boolean</param>
        public Value(bool value)
        {
            this.Type = ValueType.Real;
            this.Real = value?TrueValue:FalseValue;
        }

        /// <summary>
        /// Constructor with a string as initial value
        /// </summary>
        /// <param name="str">the string to initialize with</param>
        public Value(string str): this()
        {
            this.Type = ValueType.String;
            this.String = str;
        }

        /// <summary>
        /// Constructor with an object as initial value. The description is used when the value is converted to string
        /// FBasic cannot manipulate objects, but it can store and retrieve them; the manipulation is responsibility of the host application and the libraries
        /// </summary>
        /// <param name="obj">The object to store</param>
        /// <param name="description"></param>
        public Value(Object obj, string description)
        {
            this.Type = ValueType.Object;
            this.Object = obj;
            this.String = description;
        }


        /// <summary>
        /// Constructor with a DateOnly as initial value that is converted to string using the interpreter date format
        /// </summary>
        /// <param name="dateOnly"></param>
        public Value(DateOnly dateOnly)
        {
            this.Type = ValueType.String;
            this.String = dateOnly.ToString(dateFormat);
        }


        /// <summary>
        /// Constructor with a TimeOnly as initial value that is converted to string using the interpreter time format
        /// </summary>
        /// <param name="timeOnly"></param>
        public Value(TimeOnly timeOnly)
        {
            this.Type = ValueType.String;
            this.String = timeOnly.ToString(timeFormat);
        }


        /// <summary>
        /// Constructor that will copy type and value from another value
        /// </summary>
        /// <param name="value">The value to copy</param>
        public Value(Value value)
        {
            this.Type=value.Type;
            this.Real=value.Real;
            this.String=value.String;
        }

        /// <summary>
        /// Convert from the current type to the specified
        /// </summary>
        /// <param name="type">Type to convert to</param>
        /// <returns>A Value of the new type</returns>
        public Value Convert(ValueType type)
        {
            if (this.Type != type)
            {
                switch (type)
                {
                    case ValueType.Real:
                        double tmpd;
                        if (double.TryParse(this.String, out tmpd))
                        {
                            this.Real = tmpd;
                        } else
                        {   // (!) does not produce error. 
                            this.Real = 0;
                        }
                        this.Type = ValueType.Real;
                        break;
                    case ValueType.String:
                        this.String = this.Real.ToString();
                        this.Type = ValueType.String;
                        break;

                    case ValueType.Object:
                        throw new Exception(Errors.X011_CannotConvert("Object", "Any FBASIC type"));       
                }
            }
            return this;
        }

        public Value UnaryOp(Token tok)
        {
            if (Type != ValueType.Real)
            {
                throw new Exception(Errors.X007_OnlyUnaryOperationsOnNumbers());
            }
            switch (tok)
            {
                case Token.Plus: return this;
                case Token.Minus: return new Value(-Real);
                case Token.Not: return new Value(Real == 0 ? 1 : 0);
            }

            throw new Exception(Errors.X008_UnknownUnaryOperator());
        }

        public Value BinOp(Value b, Token tok)
        {
            Value a = this;
            if (a.Type != b.Type)
            {
				// promote one value to higher type
                if (a.Type > b.Type)
                    b = b.Convert(a.Type);
                else
                    a = a.Convert(b.Type);
            }

            if (tok == Token.Plus)
            {
                if (a.Type == ValueType.Real)
                    return new Value(a.Real + b.Real);
                else
                    return new Value(a.String + b.String);
            }
            else if(tok == Token.Equal)
            {
                 if (a.Type == ValueType.Real)
                     return new Value(a.Real == b.Real ? 1 : 0);
                else
                     return new Value(a.String == b.String ? 1 : 0);
            }
            else if(tok == Token.NotEqual)
            {
                 if (a.Type == ValueType.Real)
                     return new Value(a.Real == b.Real ? 0 : 1);
                else
                     return new Value(a.String == b.String ? 0 : 1);
            }
            else
            {
                if (a.Type == ValueType.String)
                {
                    var cmp=string.Compare(a.String, b.String, StringComparison.Ordinal);
                    switch (tok)
                    {
                        case Token.Less: return new Value(cmp < 0 ? 1 : 0);
                        case Token.More: return new Value(cmp > 0 ? 1 : 0);
                        case Token.LessEqual: return new Value(cmp < 0 | cmp==0 ? 1 : 0);
                        case Token.MoreEqual: return new Value(cmp > 0 | cmp==0 ? 1 : 0);

                        default:
                            throw new Exception(Errors.X009_CannotDoBinopOnStrings());
                    }

                    
                } 
                else
                {
                    switch (tok)
                    {
                        case Token.Minus: return new Value(a.Real - b.Real);
                        case Token.Asterisk: return new Value(a.Real * b.Real);
                        case Token.Slash: return new Value(a.Real / b.Real);
                        case Token.Caret: return new Value(Math.Pow(a.Real, b.Real));
                        case Token.Less: return new Value(a.Real < b.Real ? 1 : 0);
                        case Token.More: return new Value(a.Real > b.Real ? 1 : 0);
                        case Token.LessEqual: return new Value(a.Real <= b.Real ? 1 : 0);
                        case Token.MoreEqual: return new Value(a.Real >= b.Real ? 1 : 0);
                        case Token.And: return new Value((a.Real != 0) && (b.Real != 0) ? 1 : 0);
                        case Token.Or: return new Value((a.Real != 0) || (b.Real != 0) ? 1 : 0);
                    }
                }

            }

            throw new Exception(Errors.X010_UnknownBinaryOperator());
        }

        /// <summary>
        /// Convert the value to string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            if (this.Type == ValueType.Real)
                return this.Real.ToString();
            return this.String;
        }

        /// <summary>
        /// Convert an arithmetic value to integer
        /// </summary>
        /// <returns>Int</returns>
        public int ToInt()
        {
            if (this.Type == ValueType.Real) return (int)this.Real;
            throw new Exception(Errors.X011_CannotConvert("String","Integer"));
        }

        /// <summary>
        /// Convert the current arithmetic value to boolean
        /// </summary>
        /// <returns>bool</returns>
        public bool ToBool() => this.Real==FalseValue?false:true;

        /// <summary>
        /// Check if the value is false
        /// </summary>
        /// <returns>bool</returns>
        public bool IsFalse() => ToBool()==false;

        /// <summary>
        /// Check if the value is true
        /// </summary>
        /// <returns></returns>
        public bool IsTrue() => !IsFalse();

        /// <summary>
        /// Check if the value is an Object
        /// </summary>
        /// <returns></returns>
        public bool IsObject() => this.Type == ValueType.Object;

        /// <summary>
        /// Check if the value is a wildcard
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsWildcard() => this.Real == FalseValue && this.String == "*";


        /// <summary>
        ///     Compares this instance with a specified Value and indicates whether this
        ///     instance precedes, follows, or appears in the same position in the sort order
        ///     as the specified System.Object.
        /// </summary>
        /// <returns>A 32-bit signed integer that indicates the comparison between two values as follows,
        ///     Less than zero – This value is Less than  precedes value.
        ///     Zero – This value is equal as second value.
        ///     Greater than zero – This value if Greater than the value2..
        ///</returns>
        public int CompareTo(Value value2)
        {
            if ( this.Type != value2.Type )
            {
                throw new Exception($"Cannot compare different value types ({this.Type},{value2.Type}) [X000]");
            }

            switch(this.Type)
            {
                case ValueType.Real:
                    if (this.Real == value2.Real ) return 0;
                    if (this.Real < value2.Real ) return -1; 
                    return 1;

                case ValueType.String:
                    return this.String.CompareTo(value2.String);

                case ValueType.Object:
                    if (this.Object.Equals(value2.Object)) return 0;
                    if (this.Object is IComparable && value2.Object is IComparable) 
                        return ((IComparable)this.Object).CompareTo(value2.Object);
                    return int.MinValue;

                default:
                    throw new NotSupportedException();
            }

                
        }

    }
}
