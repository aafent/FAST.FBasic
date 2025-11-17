using System.Data.Common;

namespace FAST.FBasicInterpreter.DataProviders
{
    public class CursorCollection : DataReaderCollection<DbDataReader, FBasicSqlDataProvider>
    {
        public override string title => "CURSOR";

        public CursorCollection(string cursorName, FBasicSqlDataProvider callback) : base(cursorName, callback)
        {
        }

        public DbConnection channel = null;
        public DbCommand command = null;

        protected override bool readerHasRows => reader.HasRows;

        protected override void closeCollection(string name) => callback.closeCursor(name, this);

        protected override bool openCollection(string name) => callback.openCursor(name, this);
    }

}
