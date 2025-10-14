namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// FBASIC execution Results
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string errorText {  get; set; }

        /// <summary>
        /// The line of the error
        /// </summary>
        public int lineOfError {  get; set; }

        /// <summary>
        /// The source line of the error
        /// </summary>
        public string errorSourceLine { get; set; }

        /// <summary>
        /// Indicator if has or not an error
        /// </summary>
        public bool hasError {  get; set;}

        /// <summary>
        /// The result of the execution of FBASIC
        /// Statement to set: RESULT value
        /// </summary>
        public Value value { get;set; }

    #if DEBUG
        /// <summary>
        /// The exception, available only durring the DEBUG 
        /// </summary>
        public Exception exception=null;
#endif

        /// <summary>
        /// The begin of the FBASIC program run
        /// </summary>
        public DateTime programStartWhen {  get; set; }

        /// <summary>
        /// The end of the FBASIC program run
        /// </summary>
        public DateTime programEndWhen { get; set; }

        /// <summary>
        /// The interpreters variables with the values, after the execution of the program
        /// The dictionary key is the variable name and the value is the variable Value
        /// </summary>
        public Dictionary<string, Value> variables { get; set; } = null;
    }
}
