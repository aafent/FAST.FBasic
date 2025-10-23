
namespace FAST.FBasicInterpreter
{

    /// <summary>
    /// Interface for FBasic File Access Implementation
    /// </summary>
    public interface IFBasicFileDescriptor
    {
        /// <summary>
        /// Context Name Specifier 
        /// </summary>
        string Context { get; }

        /// <summary>
        /// Library Name
        /// </summary>
        string Library { get;  }

        /// <summary>
        /// Path in the library
        /// </summary>
        string Path { get; set; }   

        /// <summary>
        /// File name
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Any object can be assigned here
        /// </summary>
        object Tag {  get; set; }
    }

}
