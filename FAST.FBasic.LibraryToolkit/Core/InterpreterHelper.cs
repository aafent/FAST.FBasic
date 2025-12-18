namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Helper for the FBasic Interpreter
    /// </summary>
    public static class InterpreterHelper
    {
        /// <summary>
        /// Check if the token is a FBASIC statement
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsStatement(Token token)
        {
            switch (token)
            {
                case Token.Print:
                case Token.If:
                case Token.EndIf:
                case Token.Then:
                case Token.Else:
                case Token.For:
                case Token.To:
                case Token.Next:
                case Token.Goto:
                case Token.Input:
                case Token.Let:
                case Token.Gosub:
                case Token.Return:
                case Token.Rem:
                case Token.End:
                case Token.Assert:
                case Token.AddOn:
                case Token.Result:
                case Token.Dump:
                case Token.Call:
                case Token.Chain:
                case Token.Eval:
                case Token.ForEach:
                case Token.EndForEach:
                case Token.SData:
                case Token.RInput:
                case Token.Wait:
                    return true;

                default:
                    return false;
            }
            ;

        }

        /// <summary>
        /// Return the nearest FBasic value type of a C# type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isInteger"></param>
        /// <returns></returns>
        public static ValueType NearestValueType(Type type, out bool isInteger)
        {
            isInteger=false;
            // Handle null case
            if (type == null) return ValueType.Object;

            // Handle Nullable types (e.g., int?) by checking the underlying type
            Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

            // Check the TypeCode against all numeric types
            switch (Type.GetTypeCode(nonNullableType))
            {
                case TypeCode.String:
                case TypeCode.DateTime:
                    isInteger = false;
                    return ValueType.String;

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    isInteger=true;
                    return ValueType.Real;

                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    isInteger = false;
                    return ValueType.Real;

                case TypeCode.Boolean:
                    isInteger=true;
                    return ValueType.Real;

                default:
                    isInteger = false;
                    return ValueType.Object;
            }
        }

        /// <summary>
        /// Cast value using the nearest value to the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Object CastValue(Type type, Value value)
        {
            switch (InterpreterHelper.NearestValueType(type, out bool isInteger))
            {
                case ValueType.String:
                    return value.String;

                case ValueType.Real:
                    if (isInteger)
                    {
                        return value.ToInt();
                    }
                    else
                    {
                        return value.Real;
                    }

                case ValueType.Object:
                default:
                    return value.Object;
            }
        }

        /// <summary>
        /// Case Value using the nearest type to the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastValue<T>(Value value)
        {
            return (T)(CastValue(typeof(T), value));  
        }


    }
}
