﻿namespace FAST.FBasicInterpreter
{
    public enum ValueType
    {
        Real, // it's double
        String
    }

    public struct Value
    {
        public static readonly Value Zero = new Value(0);

        /// <summary>
        /// Used to return an Error. 
        /// Any Error casted to 0 value
        /// </summary>
        public static readonly Value Error = new Value(0);

        public ValueType Type { get; set; }

        public double Real { get; set; }
        public string String { get; set; }

        public Value(double real) : this()
        {
            this.Type = ValueType.Real;
            this.Real = real;
        }

        public Value(string str): this()
        {
            this.Type = ValueType.String;
            this.String = str;
        }

        public Value(Value value)
        {
            this.Type=value.Type;
            this.Real=value.Real;
            this.String=value.String;
        }

        public Value Convert(ValueType type)
        {
            if (this.Type != type)
            {
                switch (type)
                {
                    case ValueType.Real:
                        this.Real = double.Parse(this.String);
                        this.Type = ValueType.Real;
                        break;
                    case ValueType.String:
                        this.String = this.Real.ToString();
                        this.Type = ValueType.String;
                        break;
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
                    throw new Exception(Errors.X009_CannotDoBinopOnStrings());

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

            throw new Exception(Errors.X010_UnknownBinaryOperator());
        }

        public override string ToString()
        {
            if (this.Type == ValueType.Real)
                return this.Real.ToString();
            return this.String;
        }

        public int ToInt()
        {
            if (this.Type == ValueType.Real) return (int)this.Real;
            throw new Exception(Errors.X011_CannotConvert("String","Integer"));
        }
    }
}
