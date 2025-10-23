using FAST.FBasicInteractiveConsole;
using FAST.FBasicInterpreter;
using FAST.FBasicInterpreter.TemplatingLibrary;
using Microsoft.Extensions.Configuration;
using System.Data.Odbc;

namespace FAST.FBasic.InteractiveConsole
{
    internal partial class FBasicIC
    {
        private void setupEnvironment()
        {
            string cs = "<connection string>";
            //connectionAdapterForODBC connection = new(cs, dbDialectDetails.sql);

            this.env = new();
            env.DefaultEnvironment();
            env.callHandler = (name) => { var filepath = Path.Combine(programsFolder, name); return File.ReadAllText(filepath); };
            env.requestForObjectHandler = (context, group, name) =>
            {
                if ($"{context}.{group}.{name}" == "SQL.CONNECTION.Cust")
                {
                    string cs = "<replace with your CS hear>";
                    cs= "Driver={Adaptive Server Enterprise};NA=alpha.pca.com.gr,5000;Uid=laskaris;Pwd=vasilis;database=LASKARIS;ServerInitiatedTransactions=0;ApplicationName=VentusBUS;CodePageType=ANSI;ClientCharset=iso_1;CharSet=NoConversions;ServerInitiatedTransactions=0;AnsiNull=0";
                    var connection = new OdbcConnection(cs);
                    return connection;
                }

                if ($"{context}.{group}.{name}" == "SQL.CONNECTION.DATA1") 
                {
                    string cs = "<replace with your CS hear>";
                    var connection = new OdbcConnection(cs);
                    return connection;
                }
                if ($"{context}.{group}.{name}" == "IN.TEST.NAME") return "THIS IS AN IN TEST!";
                return null;
            };
            env.AddLibrary(new FBasicStringFunctions());
            env.AddLibrary(new FBasicMathFunctions());
            env.AddLibrary(new FBasicDateFunctions());
            env.AddLibrary(new FBasicSQLDataAdapter());
            env.AddLibrary(new FBasicEvents());
            env.AddLibrary(new FBasicTextReplacer());
            env.AddLibrary(new FBasicStack());
            env.AddLibrary(new FBasicDecisionTables());
            env.AddLibrary(new FBasic2DArrays());
            env.AddLibrary(new FBasicStreams());
            env.AddLibrary(new FBasicTemplatingLibrary());
            env.AddVariable("table.column", "myColumn1");

            FBasicEvents.Reset();
            FBasicEvents.FBasicEventHandler += testEventHandler1!;
            FBasicEvents.FBasicEventHandler += testEventHandler2!;
        }

    }
}
