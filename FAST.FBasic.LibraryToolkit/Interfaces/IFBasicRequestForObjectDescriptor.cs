namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Request For Object Descriptor
    /// </summary>
    public interface IFBasicRequestForObjectDescriptor
    {
        /// <summary>
        /// The requestor interpreter
        /// </summary>
        IInterpreter Interpreter { get; set; }

        /// <summary>
        /// The context of the request
        /// </summary>
        string Context { get; set; }

        /// <summary>
        /// The Group of the request
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// The name of the Request
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Optional, the variable name that will store the request
        /// </summary>
        string VariableName { get; set; }
    }
}
