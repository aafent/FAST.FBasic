namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// File Descriptor for FBasic source programs
    /// </summary>
    public class FBasicSourceProgramFile : IFBasicFileDescriptor
    {
        public string Context {get; set; } = "FBASIC.PROGRAM";
        public string Library {get; set; } = ".";
        public string Path { get; set;  } = ".";
        public string FileName { get; set;  } = null;
        public object Tag { get; set; } = null;


        public FBasicSourceProgramFile(string programName)
        {
            this.FileName= programName;
        }


    }
}
