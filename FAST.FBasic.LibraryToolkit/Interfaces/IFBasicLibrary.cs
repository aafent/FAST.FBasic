
namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Interface for FBASIC Libraries 
    /// </summary>
    public interface IFBasicLibrary
    {
        void InstallAll(IInterpreter interpreter);

        //abstract static IFBasicLibrary Library { get; }
    }
}
