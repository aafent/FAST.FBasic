using System.Runtime.InteropServices;

namespace FAST.FBasicInterpreter
{
    public interface IFBasicFileManagementLayer
    {
        /// <summary>
        /// The File Handler for the FBasic interpreter
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public IFBasicFileManagementLayer Handler(IFBasicFileDescriptor file);


        /// <summary>
        /// The requested file descriptor
        /// </summary>
        public IFBasicFileDescriptor RequestedFile {  set; }

        /// <summary>
        /// Return the FBasic Source Program
        /// </summary>
        /// <param name="name">The name of the program with or without an extension usually ".bas" </param>
        /// <returns>the FBasic source program</returns>
        public string GetSourceProgram();

        /// <summary>
        /// Get a read only file stream
        /// </summary>
        /// <returns></returns>
        public FileStream ReadOnlyFileStream();

        /// <summary>
        /// Get an output file stream
        /// </summary>
        /// <param name="fileShare"></param>
        /// <returns></returns>
        public FileStream OutputFileStream(FileShare fileShare);

    }


}
