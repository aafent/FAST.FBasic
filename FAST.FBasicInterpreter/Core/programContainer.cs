namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Contain of a parsed FBASIC program 
    /// </summary>
    public class programContainer
    {
        /// <summary>
        /// An array of the parsed program elements
        /// </summary>
        public programElement[] elements { get; set; }

        /// <summary>
        /// The variable names used in the program
        /// </summary>
        public Dictionary<string, ValueType> variables {  get; set; }
        
    }


    /// <summary>
    /// A single FBASIC program element
    /// </summary>
    public class programElement
    {
        /// <summary>
        /// The token
        /// </summary>
        public Token token {get; set; }

        /// <summary>
        /// The value (any type)
        /// </summary>
        public Value value {get; set; }

        /// <summary>
        /// The identifier
        /// </summary>
        public string identifier {get;set; }

        /// <summary>
        /// true if the identifier is doted (class.property)
        /// </summary>
        public bool isDoted { get; set; } 


        /// <summary>
        /// Copy the values from the source
        /// </summary>
        /// <param name="source">The source</param>
        public void copyFrom(programElement source)
        {
            this.token = source.token;
            this.value = source.value;
            this.identifier = source.identifier;
            this.isDoted = source.isDoted;
        }
    }

}
