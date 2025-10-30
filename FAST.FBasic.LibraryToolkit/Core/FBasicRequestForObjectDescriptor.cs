namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Request For Object Descriptor
    /// </summary>
    public class FBasicRequestForObjectDescriptor : IFBasicRequestForObjectDescriptor
    {
        #region (+) Constructors 

        public FBasicRequestForObjectDescriptor()
        {
        }

        public FBasicRequestForObjectDescriptor(IInterpreter interpreter):this()
        {
            this.Interpreter= interpreter;  
        }

        public FBasicRequestForObjectDescriptor(IInterpreter interpreter, string context) : this(interpreter)
        {
            this.Context= context;  
        }

        public FBasicRequestForObjectDescriptor(IInterpreter interpreter, string context,string group) : this(interpreter,context)
        {
            this.Group= group;
        }

        public FBasicRequestForObjectDescriptor(IInterpreter interpreter, string context, string group, string name) : this(interpreter, context, group)
        {
            this.Name = name;
        }
        #endregion (+) Constructors 

        /// <summary>
        /// The requestor interpreter
        /// </summary>
        public IInterpreter Interpreter { get; set; }

        /// <summary>
        /// The context of the request
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// The Group of the request
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The name of the Request
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional, the variable name that will store the request
        /// </summary>
        public string VariableName { get; set; } = null;
    }
}
