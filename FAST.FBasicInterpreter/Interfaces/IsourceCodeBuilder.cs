namespace FAST.FBasicInterpreter
{
    public interface IsourceCodeBuilder
    {
        void Build(programContainer program);
        string GetSource();
    }
}