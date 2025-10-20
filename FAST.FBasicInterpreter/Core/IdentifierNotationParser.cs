
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Parsing an square bracket [] surrounded identifier
    /// brackets can exists or not.
    /// Syntax:
    /// [col.Item]          ::  the of the current item from the static (SDIM) collection
    /// [col.column]        ::  the value of the column from the collection
    /// [arr.column(index)] ::  the column of the array at the specified index
    /// </summary>
    public class IdentifierNotationParser
    {
        private readonly Interpreter interpreter;

        /// <summary>
        /// True if the expression notate an Array
        /// </summary>
        public bool IsArray {  get; private set; } = false;

        /// <summary>
        /// True if the expression is for collection
        /// </summary>
        public bool IsCollection {  get; private set; } = false;

        /// <summary>
        /// True if the expression is a function name
        /// </summary>
        public bool IsFunction {  get; private set; } =false;


        /// <summary>
        /// The expression for the index of the array
        /// </summary>
        public string IndexExpression {  get; private set;  } = null;

        /// <summary>
        /// The index for the array
        /// </summary>
        public int ArrayIndex {  get; private set; } = -1;

        /// <summary>
        /// The data container/provider such Collection, Array, etc
        /// </summary>
        public string DataContainerName {  get; private set; } = null;

        /// <summary>
        /// The data element in the container such Column, Field, Item etc
        /// </summary>
        public string DataElement {  get; private set; } = null;

        /// <summary>
        /// Data element number, convert to number the data element
        /// </summary>
        public int DataElementAsNumber
        {
            get
            {
                return int.Parse(this.DataElement);
            }
        }

        /// <summary>
        /// Check if the data element is a numeric value
        /// </summary>
        public bool IsDataElementNumeric
        {
            get
            {
                if (string.IsNullOrEmpty(this.DataElement)) return false;
                return int.TryParse(this.DataElement, out _);
            }
        }

        /// <summary>
        /// The function name if the expression is a function name
        /// </summary>
        public string FunctionName {  get; private set; } = null;


        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="expression"></param>
        public IdentifierNotationParser(string expression, Interpreter interpreter=null)
        {
            this.interpreter=interpreter;
            parseExpression(expression);
            standardChecks();
        }


        private void standardChecks()
        {
            if (interpreter == null) return;


            if (this.IsArray )
            {
                if (!interpreter.IsArray(this.DataContainerName) )
                {
                    interpreter.Error("[]",Errors.E111_UndeclareIdentifier(this.DataElement));
                    return;
                }
            }
            else if (this.IsCollection ) 
            {
                if (this.DataElement.ToUpper() != "ITEM")
                {
                    if (!interpreter.collections.ContainsKey(this.DataElement))
                    {
                        interpreter.Error("[]", Errors.E111_UndeclareIdentifier(this.DataElement));
                        return;
                    }
                }
            }

        }

        private void parseExpression(string expression)
        {

            // (v) remove the surrounding [] if the exists
            if (expression.Length > 1 && expression[0] == '[' && expression[expression.Length - 1] == ']')
            {
                expression = expression.Substring(1, expression.Length - 2);
            }

            #region (+) Parse the IndexArray and IndexExpression
            var leftParenthesisPos = expression.IndexOf('(');
            if (leftParenthesisPos>=0 )
            {
                var rightParenthesisPos = expression.IndexOf(')',leftParenthesisPos+1);
                if (rightParenthesisPos<0)
                {
                    error("Left parenthesis does not closing with a Right");
                    return;
                }
                IndexExpression = expression.Substring(leftParenthesisPos+1, (rightParenthesisPos - leftParenthesisPos)-1 ).Trim();
                expression=expression.Substring(0, leftParenthesisPos  ).Trim();
                this.IsArray=true;
                if (string.IsNullOrEmpty(IndexExpression))
                {
                    error("No index specified.");
                    return;
                }

                if (char.IsDigit(IndexExpression[0]) ) // if the first character is a numeric 
                {
                    if (!int.TryParse(IndexExpression, out int tmpi))
                    {
                        error("Wrong index number.");
                        return;
                    }
                    ArrayIndex=tmpi;
                } 
                else
                {
                    if (interpreter!=null) ArrayIndex=interpreter.GetVar(IndexExpression).ToInt();
                }
            }
            #endregion (+) Parse the IndexArray and IndexExpression


            var dotPosition = expression.IndexOf('.');
            if (dotPosition >= 0)
            {
                if (expression.IndexOf('.',dotPosition+1) >= 0  )
                {
                    error("Unsupported multiple reference.");
                    return;
                }
                this.DataContainerName=expression.Substring(0,dotPosition );
                this.DataElement = expression.Substring(dotPosition + 1);
            
                if (!this.IsArray)
                {
                    if (interpreter.IsFunction(this.DataContainerName))
                    {
                        this.IsFunction=true;   
                        this.FunctionName=this.DataContainerName;
                        this.DataContainerName = null;
                    } 
                    else
                    {
                        this.IsCollection = true;
                    }
                }
            }
            else // (<) No dot position
            {
                if (interpreter.IsFunction(expression))
                {
                    this.IsFunction = true;
                    this.FunctionName = expression;
                    this.DataContainerName = null;
                }
            }

        }

        private void error(string what)
        {
            if (interpreter != null)
            {
                interpreter.Error("[]",Errors.E134_SquareBracketNotation(what));
                return;
            }
            else
            {
                throw new Exception(Errors.E134_SquareBracketNotation(what));
            }          
        }

    }
}
