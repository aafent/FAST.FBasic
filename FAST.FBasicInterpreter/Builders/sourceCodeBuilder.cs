namespace FAST.FBasicInterpreter.Builders
{
    public abstract class SourceCodeBuilderAbstract : ISourceCodeBuilder
    {
        private Dictionary<Token,int> tokenReferences =new();

        protected abstract ProgramContainer program { get; set; }

        public abstract void Build(ProgramContainer program);

        public abstract string GetSource();


        protected bool hasLogicalOperators { get; private set; }
        protected bool hasMathExpression { get; private set; }
        protected bool hasValue { get; private set; }
        protected bool hasIdentifier { get; private set; }
        protected bool hasVariable { get; private set; }
        protected bool hasAssignment { get; private set; }
        protected bool isStatement { get; private set; }
        protected int numOfStringValues { get; private set; }
        protected int numOfRealValues { get; private set; }
        protected int numOfEqualSigns { get; private set; }


        /// <summary>
        /// A readable format of an element
        /// Used for debug
        /// </summary>
        /// <param name="element">The element</param>
        /// <returns>Readable string</returns>
        protected string readableElement(ProgramElement element)
        {
            return $"[{element.token} ({element.identifier}, {element.value})]";
        }




        protected void resetAnalysis()
        {
            this.hasLogicalOperators = false;
            this.hasMathExpression = false;
            this.hasValue = false;
            this.hasIdentifier = false;
            this.hasVariable = false;
            this.hasAssignment = false;
            this.isStatement=false;
            this.numOfRealValues = 0;
            this.numOfStringValues = 0;
            this.numOfEqualSigns = 0;
        }
        protected void lineAnalysis(int currentTokenNumber)
        {
            resetAnalysis();
            int startIndex= currentTokenNumber-1; // currentTokenNumber starts from 1
            if (startIndex < 0) return;
            if (currentTokenNumber >= program.elements.Length) return;

            bool exitAnalysis=false;
            var prevIndex = startIndex;
            for(var index=startIndex; index < program.elements.Length; index++)
            {
                var element= program.elements[index];
                if ( index== startIndex) // first time
                {
                    prevIndex = index;
                } 
                var prevElement = program.elements[prevIndex];

                this.isStatement=InterpreterHelper.IsStatement(element.token);
                if (!this.isStatement) switch (element.token)
                {
                    case Token.Plus:
                    case Token.Minus:
                    case Token.Slash:
                    case Token.Asterisk:
                    case Token.Caret:
                    case Token.LParen:
                    case Token.RParen:
                        this.hasMathExpression=true;
                        break;

                    case Token.Identifier:
                        this.hasIdentifier=true;
                        if (program.variables.ContainsKey(element.identifier) )
                        {
                            this.hasVariable=true;
                        }
                        break;

                    case Token.Value:
                        this.hasValue=true;
                        if (element.value.Type == ValueType.String) 
                            this.numOfStringValues++;
                                else this.numOfRealValues++;
                        break;

                    case Token.Equal:
                        if ( prevElement.token == Token.Identifier )
                        {
                            if (hasVariable ) hasAssignment=true;
                        }
                        numOfEqualSigns++;
                        break;

                    case Token.Less:
                    case Token.More:
                    case Token.NotEqual:
                    case Token.LessEqual:
                    case Token.MoreEqual:
                    case Token.Or:
                    case Token.And:
                    case Token.Not:
                        this.hasLogicalOperators=true;
                        break;

                    case Token.EOF:
                    case Token.NewLine:
                        // reach end of command or program or line
                        exitAnalysis=true;
                        break;
                }

                if (exitAnalysis) break;
                prevIndex = index;
                
            } // for

            return;
        }

        protected void makeReference(Token token)
        {
            if (tokenReferences.ContainsKey(token)) 
            {
                tokenReferences[token]++;
            } 
            else
            {
                tokenReferences.Add(token, 1);
            }
        }
        protected void removeReference(Token token)
        {
            if (tokenReferences.ContainsKey(token))
            {
                tokenReferences[token]--;
                if (tokenReferences[token] < 1) tokenReferences[token]=0;
            }
        }
        protected bool isReferenced(Token token)
        {
            if (!tokenReferences.ContainsKey(token)) return false;
            if (tokenReferences[token] > 0 ) return true;
            return false;
        }
        
    }
}
