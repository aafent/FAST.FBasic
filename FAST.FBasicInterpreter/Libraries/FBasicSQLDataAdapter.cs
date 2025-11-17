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
        public void InstallAll(IInterpreter interpreter)
        {
            interpreter.AddDataAdapter(new FBasicSqlDataProvider());

            // (>) in this case, the data adapter is also a library, so it will add the functions and statements itself
        }



    }

}
