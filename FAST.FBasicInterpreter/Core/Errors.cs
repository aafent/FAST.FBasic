namespace FAST.FBasicInterpreter
{
    public static class Errors
    {
        public static string E102_ExpectingThatButGotThis(string expected, string found)
        {
            return $"Expecting {expected}, got {found} [E102]";
        }
        public static string E106_ExpectingKeyword(string found, string msg="")
        {
            return $"Expecting keyword/statement, got {found}({msg}) [E106]";
        }

        public static string E107_LabelNotFound(string label)
            => $"Cannot find label named {label} [E107]";


        public static string E117_CollectionIsNotSDATAType(string collectionName)
        {
            return $"Collection {collectionName} is not SDATA type of collection [E117]";
        }

        public static string E124_NotPermitted(string forbittenItem)
        {
            return $"{forbittenItem} not permitted. Use LET to assign the result value and use the variable [E124]";
        }
        public static string E112_UndeclaredVariable(string varName)
        {
            return $"Undeclared variable {varName} [E112]";
        }
        public static string E125_WrongNumberOfArguments(int argNo, string syntax = "")
        {
            // Wrong number of arguments. Expected {argNo}. {syntax} [E125]
            string msg;
            if (argNo==0)
            {
                msg = $"Wrong number of arguments. No arguments are expected.";
            } else
            {
                msg = $"Wrong number of arguments. Expected {argNo}.";
            }
            
           
            if (!string.IsNullOrEmpty(syntax)) msg += syntax;
            msg += " [E125]";
            return msg;
        }

        public static string E132_InternalError(string moreText = null)
        {
            return $"Internal error. {moreText} [E132]";
        }



        public static string E126_WrongArgumentType(int argNo=0, string syntax = "")
        {
            // Wrong argument(s) type. Check argument: {argNo}. {syntax} [E126]
            string msg = $"Wrong argument(s) type. ";
            if (argNo>0) msg+="Check argument: {argNo}";
            if (!string.IsNullOrEmpty(syntax)) msg += syntax;
            msg += " [E126]";
            return msg;
        }
        public static string E127_WrongArgumentReferredType(string expectedType, string syntax = "")
        {
            // Wrong referred type. Expected: {expectedType}. {syntax} [E127]
            string msg = $"Wrong referred type. Expected: {expectedType} ";
            if (!string.IsNullOrEmpty(syntax)) msg += syntax;
            msg += " [E126]";
            return msg;
        }

        public static string E128_RequestHandlerNullReturn(string context, string group, string name, string moreText=null)
        {
            return $"Request object error. An object expected but got null. Request:{context}.{group}.{name}. {moreText} [E128]";
        }

        public static string E130_OutOfRange(string whatIsOutOfRange, string more=null)
        {
            return $"{whatIsOutOfRange} is out of range. {more} [E129]";
        }

        public static string E129_IsEmpty(string whatIsEmpty)
        {
            return "{whatIsEmpty} is empty [E129]";
        }

        public static string X007_OnlyUnaryOperationsOnNumbers()
        {
            return "Can only do unary operations on numbers. [X007]";
        }

        public static string X008_UnknownUnaryOperator() 
        {
            return "Unknown unary operator. [X008]";
        }

        public static string X009_CannotDoBinopOnStrings()
        {
            return "Cannot do binop on strings(except +). [X009]";
        }

        public static string X010_UnknownBinaryOperator()
        {
            return "Unknown binary operator. [X010]";
        }
    
        public static string X011_CannotConvert(string fromType, string toType)
        {
            return $"Cannot convert {fromType} to {toType}. [X011]";
        }

        public static string X012_GeneralException(string source, Exception ex)
        {
            string msg = $"FATAL Error ({source}) {ex.Message}. ";
            if (ex.InnerException !=null) msg+=$"{ex.InnerException.Message} [X012]";
            return msg;
        }

    }

}
