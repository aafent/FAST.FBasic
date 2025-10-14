using System.Data;

namespace FAST.FBasicInterpreter.DataProviders
{
    /// <summary>
    /// Abstract class to implement any collection based on a data reader
    /// </summary>
    /// <typeparam name="TReader">The type of the reader</typeparam>
    /// <typeparam name="TCallback">The type of FBASIC data adapter callback</typeparam>
    public abstract class dataReaderCollection<TReader, TCallback> : IBasicCollection
        where TReader : IDataReader
        where TCallback : IFBasicDataAdapter
    {
        public bool _endOfData = false;
        public bool endOfData
        {
            get => _endOfData;
            set => _endOfData = value;
        }

        public abstract string title { get; }

        public TReader reader;

        private bool isOpen = false;
        protected TCallback callback;
        private string cursorName = null;
        private Dictionary<string, Tuple<int, Type, bool>> columnsMap = null;

        public dataReaderCollection(string cursorName, TCallback callback)
        {
            this.callback = callback;
            this.cursorName = cursorName;
            inner_reset();
        }

        public object Current { get; set; } = null;

        protected abstract bool openCollection(string name);
        protected abstract void closeCollection(string name);
        protected abstract bool readerHasRows { get; }


        public bool MoveNext()
        {
            if (!this.isOpen)
            {
                this.isOpen = openCollection(cursorName); //callback.openCursor(cursorName, this);
                if (!this.isOpen) return false; //moveToNext=false 
                this.columnsMap = getColumnsMap(reader);
            }

            // (v) code to MoveNext()
            if (!readerHasRows) return false; //moveToNext=false;
            this.Current = reader;

            this.endOfData = !reader.Read();

            return !this.endOfData;
        }

        public void Reset()
        {
            if (this.isOpen)
            {
                closeCollection(cursorName); //callback.closeCursor(cursorName, this);
                this.isOpen = false;
            }

            inner_reset();
        }

        private void inner_reset()
        {
            this.endOfData = false;
            this.isOpen = false;
        }

        public Value getValue(string name)
        {
            if (!this.columnsMap.ContainsKey(name)) callback.Error(title, $"Name: {name} is missing from {cursorName} [S003]");
            var value = reader[name];
            var isNumeric = columnsMap[name].Item3;
            if (isNumeric)
            {
                return new Value(Convert.ToDouble(value));
            }
            else
            {
                return new Value(value.ToString());
            }

        }


        /// <summary>
        /// A map between column name and ordinary and type 
        /// of each column, extracted from a reader
        /// MAP: Key=Column Name, Value=int:Ordinary Number, type:The Type, bool:true if type is Numeric
        /// </summary>
        /// <param name="reader">the reader</param>
        /// <returns>Dictionary with column names as key and Tuple as value</returns
        private static Dictionary<string, Tuple<int, Type, bool>> getColumnsMap(IDataReader reader)
        {
            if (reader == null) { throw new ArgumentNullException("Class isn't yet bind with a DbDataReader object"); }

            Dictionary<string, Tuple<int, Type, bool>> map = new();

            for (int field = 0; field < reader.FieldCount; field++)
            {
                var type = reader.GetFieldType(field);
                map.Add(reader.GetName(field), new Tuple<int, Type, bool>(field, type, isNumericType(type)));
            }
            return map;
        }

        /// <summary>
        /// Check if a type is numeric type
        /// </summary>
        /// <param name="type">the type to check</param>
        /// <returns>True if it is numeric type</returns>
        private static bool isNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

    }
}
