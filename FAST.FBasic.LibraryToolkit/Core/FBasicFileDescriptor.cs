namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Generic File Descriptor for FBasic file operations
    /// </summary>
    public class FBasicFileDescriptor : IFBasicFileDescriptor
    {
        public string Context {get; set; } = "FILE";
        public string Library {get; set; } = ".";
        public string Path { get; set;  } = ".";
        public string FileName { get; set;  } = null;
        public object Tag { get; set; } = null;

        /// <summary>
        /// No arguments constructor
        /// </summary>
        public FBasicFileDescriptor()
        {

        }

        /// <summary>
        /// Constructor with the file name as argument
        /// </summary>
        /// <param name="fileName">The file name</param>
        public FBasicFileDescriptor(string fileName)
        {
            this.FileName=fileName;
        }

    }
}
