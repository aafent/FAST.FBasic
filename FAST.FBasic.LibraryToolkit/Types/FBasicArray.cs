namespace FAST.FBasicInterpreter
{
    public class JaggedArray2D<T>
    {
        /// <summary>
        /// The underling data
        /// </summary>
        protected T[][] m_values=null;

        /// <summary>
        /// Initial number of columns
        /// </summary>
        public int InitialNumberOfColumns = 0;

        /// <summary>
        /// No arguments constructor
        /// </summary>
        public JaggedArray2D()
        {
        }

        /// <summary>
        /// Clone an array
        /// </summary>
        /// <param name="values"></param>
        public JaggedArray2D(T[][] values)
        {
            m_values = values;
        }

        public T this[int index0, int index1]
        {
            get => GetValue(index0, index1);

            set
            {
                SetValue(value, index0, index1);
            }
        }

        /// <summary>
        /// Sets the value at the specified indices.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index0"></param>
        /// <param name="index1"></param>
        public void SetValue(T value, int index0, int index1)
        {
            if (m_values == null) m_values = new T[0][];
            while ((m_values.Length - 1 < index0))
            {
                Array.Resize(ref m_values, m_values.Length + 1);
            }

            if (m_values[index0] == null) m_values[index0] = new T[InitialNumberOfColumns];
            while ((m_values[index0].Length - 1 < index1))
            {
                Array.Resize(ref m_values[index0], m_values[index0].Length + 1);
            }

            m_values[index0][index1] = value;
            
        }

        /// <summary>
        /// Gets the value at the specified indices.
        /// </summary>
        /// <param name="index0"></param>
        /// <param name="index1"></param>
        /// <returns></returns>
        public virtual T GetValue(int index0, int index1) => m_values[index0][index1];


        /// <summary>
        /// The Length of the  array
        /// </summary>
        public int Length => m_values?.Length??0;

        /// <summary>
        /// Get the second's level item length
        /// </summary>
        /// <param name="index">Index to the first array</param>
        /// <returns>Array of T</returns>
        public int ItemLength(int index) => m_values[index].Length;

        /// <summary>
        /// Get the second's level item
        /// </summary>
        /// <param name="index">Index to the first array</param>
        /// <returns></returns>
        public T[] GetItem(int index) => m_values[index];

        /// <summary>
        /// Remove the last array's (first array) item
        /// </summary>
        public void RemoveLastArrayItem()
        {
            if (m_values.Length < 1 ) return;
            Array.Resize(ref m_values, m_values.Length - 1);
        }
  
        /// <summary>
        /// Copies the contents of this jagged array to another jagged array.
        /// </summary>
        /// <param name="map"></param>
        public void CopyTo(JaggedArray2D<T> map)
        {
            for (int x = 0; x < this.Length; x++)
            {
                for (int y = 0; y < m_values[0].Length; y++)
                {
                    map.SetValue(GetValue(x, y), x, y);
                }
            }
        }

        /// <summary>
        /// Reset the array, delete all the content. 
        /// </summary>
        public virtual void ResetArray()
        {
            m_values=null;
        }
    }


    /// <summary>
    /// FBasic Array 
    /// </summary>
    public class FBasicArray : JaggedArray2D<Value>, IBasicCollection
    {
        private string[] columnNames = new string[0] ;

        /// <summary>
        /// Default is false. If it is true, on delete row statement, the row order will prevented
        /// but the operation will be slow, as all the rows will shifted one up. 
        /// </summary>
        public bool KeepOrderOnDeleteRow {  get; set; } = false;

        /// <summary>
        /// 0-Based index of the column
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetColumnIndex(string name)
        {
            int index=Array.IndexOf(columnNames,name);
            return index;
        }

        /// <summary>
        /// Get column number (1-Based) by name
        /// </summary>
        /// <param name="name">The column name</param>
        /// <returns></returns>
        public int GetColumnNumber(string name) => 1+GetColumnIndex(name);

        /// <summary>
        /// Return Get column name by Column number (1-based)
        /// </summary>
        /// <param name="columnNo">The 1-based column number</param>
        /// <returns></returns>
        public string GetColumnName(int columnNo) => columnNames[columnNo-1];

        /// <summary>
        /// Column names count
        /// </summary>
        public int ColumnNamesCount => this.columnNames.Length;

        /// <summary>
        /// Access the array with row (1-Based) and column
        /// </summary>
        /// <param name="row">1-Based row number</param>
        /// <param name="column">Column name</param>
        /// <returns></returns>
        public Value this[int row, string column]
        {
            get => GetValue(row - 1, GetColumnIndex(column));

            set
            {
                SetValue(value, row - 1, GetColumnIndex(column));
            }
        }

        /// <summary>
        /// Sets the value at the specified indices.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="row">Row number (1-Base)</param>
        /// <param name="columnName"></param>
        public void SetValue(Value value, int row, string columnName)
        {
            this[row,columnName]=value;
        }


        /// <summary>
        /// Get Value 
        /// </summary>
        /// <param name="index0"></param>
        /// <param name="index1"></param>
        /// <returns></returns>
        public override Value GetValue(int index0, int index1)
        {
            if (m_values == null ) return Value.Empty;
            if (m_values[index0] == null ) return Value.Empty;
            var value = m_values[index0][index1];
            return value;
        }


        /// <summary>
        /// Set the name of a column. 1-Based Numbering
        /// </summary>
        /// <param name="columnNo">Column number, 1-Based Numbering</param>
        /// <param name="name"></param>
        public void SetColumnName(int columnNo, string name)
        {
            var index = columnNo - 1; // columns are 1-Based and array 0-Based
            if (index+1 > columnNames.Length) Array.Resize(ref columnNames,1+index);
            columnNames[index]=name;
        }

        /// <summary>
        /// Check if a name is column.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsColumn(string name)
        {
            return GetColumnIndex(name) > -1;
        }

        /// <summary>
        /// Delete a row. Row number is 1-based numbering.
        /// </summary>
        /// <param name="rowToDelete">1-Based number of row</param>
        public void DeleteRow(int rowToDelete)
        {
            if (rowToDelete > this.Length || rowToDelete < 1 ) return; // no rows found or invalid index

            if (rowToDelete == this.Length) // (<) we are lucky, delete the last row
            {
                this.RemoveLastArrayItem();
                return;
            }

            int indexToDelete = rowToDelete - 1; // 1-based to 0-based

            // (v) Method: Swap with Last Element (If Order Doesn't Matter)
            if ( !KeepOrderOnDeleteRow )
            {
                var lastIndex1 = ItemLength(indexToDelete);
                for (int index1 = 0; index1<lastIndex1; index1++)
                {
                    SetValue(   GetValue(this.Length - 1,index1)   , indexToDelete, index1);
                }
                this.RemoveLastArrayItem();
                return;
            } 
            else // (v) Slow delete row by shifting up all the rest rows, but will keep the order of them
            {
                for (int y = indexToDelete +  1; y<Length; y++)
                {
                    var lastIndex = ItemLength(y);
                    for (int index = 0; index < lastIndex ; index++)
                    {
                        SetValue( GetValue(y, index), y-1, index);
                    }

                }
                this.RemoveLastArrayItem();
                return;
            }

        }

        /// <summary>
        /// Reset the array, delete all the content. But not the columns
        /// </summary>
        public override void ResetArray()
        {
            ResetColumnNames(); // reset the column names
            this.Reset(); // reset the IEnumerator part of the class
            base.ResetArray(); // reset the jagged array
        }

        /// <summary>
        /// Reset column names
        /// </summary>
        public void ResetColumnNames()
        {
            columnNames = new string[0];
        }

        /// <summary>
        /// The current row within a FOREACH loop (only) 
        /// </summary>
        /// <returns>1-based index</returns>
        public int GetCurrentRow()
        {
            if (this.currentRow < 1) return 1;
            if (this.currentRow > this.Length) return this.Length;
            return this.currentRow;
        }

        #region (+) IFBasicCollection Interface

        private int currentRow = 0;

        public bool endOfData { get => currentRow>this.Length ; set { } }

        public object Current => m_values[currentRow-1];

        public Value getValue(string name)=>this[currentRow,name]; 

        public bool MoveNext()
        {
            currentRow++;
            if (currentRow > this.Length )
            {
                currentRow = this.Length+1;
            }
            return endOfData;
        }

        public void Reset()
        {
            this.currentRow=0;
        }

        public void ClearCollection()
        {
            Reset();
            ResetArray();
        }


        #endregion (+) IFBasicCollection Interface
    }

}
