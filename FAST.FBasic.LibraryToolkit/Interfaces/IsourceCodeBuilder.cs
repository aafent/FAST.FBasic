namespace FAST.FBasicInterpreter
{
    public interface IsourceCodeBuilder
    {
        void Build(ProgramContainer program);
        string GetSource();
    }
}