using FAST.FBasicInterpreter.DataProviders;

namespace FAST.FBasicInterpreter
{
/*
SQL Data Provider:
Statements:
    CURSOR : Used to define an SQL Data cursor as collection in the FBASIC interpreter

Functions:
    sql() : Return the input Value surrounded by quotes if is a string or without if it is a number
 */
    public class FBasicSQLDataAdapter : IFBasicLibrary
    {
        public void InstallAll(Interpreter interpreter)
        {
            interpreter.AddDataAdapter(new sqlFBasicDataProvider());
        }

    }

}
