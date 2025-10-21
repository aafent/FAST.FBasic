
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Interface for FBASIC Libraries with privet memory
    /// </summary>
    public interface IFBasicLibraryWithMemory: IFBasicLibrary
    {
        /// <summary>
        /// internal unique name of the library
        /// </summary>
        string uniqueName { get; }

        /// <summary>
        /// Clear the memory of the library
        /// </summary>
        void ClearMemory();

        /// <summary>
        /// Prepare the library to execute a new program
        /// </summary>
        void PrepareToExecute()
        {
            ClearMemory();
        }
    }
}
