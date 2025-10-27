namespace FAST.FBasicInterpreter.Builders
{
    public interface ISourceCodeBuilder
    {
        void Build(ProgramContainer program);
        string GetSource();
    }
}