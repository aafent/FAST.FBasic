using System.Data.Common;

namespace FAST.FBasicInterpreter
{
    public class cursorCollection : dataReaderCollection<DbDataReader, sqlFBasicDataProvider>
    {
        public override string title => "CURSOR";

        public cursorCollection(string cursorName, sqlFBasicDataProvider callback) : base(cursorName, callback)
        {
        }

        public DbConnection channel = null;

        protected override bool readerHasRows => reader.HasRows;

        protected override void closeCollection(string name) => callback.closeCursor(name, this);

        protected override bool openCollection(string name) => callback.openCursor(name, this);
    }

}
