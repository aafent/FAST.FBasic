namespace FAST.FBasicInterpreter.DataProviders
{
    internal class staticDataCollection : IBasicCollection
    {
        public List<Value> data = null; 
        private const int indexInitialValue = (-1);
        private int index= indexInitialValue;
        private readonly IInterpreter interpreter=null;

        private int lastIndex
        {
            get
            {
                return (data.Count - 1);
            }
        }

        public bool endOfData
        {
            get
            {
                return index > lastIndex;
            }
            set { }
        }

        public staticDataCollection(IInterpreter interpreter)
        {
            this.interpreter = interpreter;
            this.data=new();
            Reset();
        }


        public object Current => this.data[index];

        public Value getValue(string name)
        {
            if (name.ToUpper() != "ITEM" )
            {
                interpreter.Error("SDATACollection","SDATA collections supporting only the field name ITEM [E119].");
                return Value.Error;
            }
            if (index<0 || index>=this.data.Count)
            {
                interpreter.Error("SDATACollection", $"Collection for {name} is empty/out-of-ForEachLoop [E120].");
                return Value.Error;
            }
            return this.data[index];
        }

        public bool MoveNext()
        {
            if (index < indexInitialValue) index= indexInitialValue; // just in case...
            index++;
            if (index > lastIndex)
            {
                index = lastIndex + 1;
                return false;
            }
            return true;
        }

        public void Reset()
        {
            index= indexInitialValue;
        }

        public void ClearCollection()
        {
            Reset();
            data.Clear();
        }
    }
}
