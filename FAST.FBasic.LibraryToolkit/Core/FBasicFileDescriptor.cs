namespace FAST.FBasicInterpreter
{
    public class FBasicFileDescriptor : IFBasicFileDescriptor
    {
        public string Context {get; set; } = "FILE";
        public string Library {get; set; } = ".";
        public string Path { get; set;  } = ".";
        public string FileName { get; set;  } = null;
        public object Tag { get; set; } = null;
    }
}
