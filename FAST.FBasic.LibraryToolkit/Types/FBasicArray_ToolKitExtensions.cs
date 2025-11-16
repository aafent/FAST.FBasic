using System.Data;

namespace FAST.FBasicInterpreter
{
    public static class FBasicArray_ToolKitExtensions
    {
        /// <summary>
        /// Convert a FBasic Array to Dictionary 
        /// </summary>
        /// <typeparam name="T1">Key type (not nullable)</typeparam>
        /// <typeparam name="T2">Value type</typeparam>
        /// <param name="array">The array to convert</param>
        /// <param name="keyIndex">The index in the array of the key value</param>
        /// <param name="valueIndex">The index in the array for the value</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<T1, T2> ConvertToDictionary<T1,T2>(this FBasicArray array, int keyIndex=0, int valueIndex=1) 
            where T1:notnull
        {
            var dictionary = new Dictionary<T1, T2>();

            T1 key;
            T2 value;

            // Get the number of rows (x dimension)
            int rows = array.Length;

            // Iterate through each row using the x dimension
            for (int x = 0; x < rows; x++)
            {
                // Use y=0 as key and y=1 as value 

                key = InterpreterHelper.CastValue<T1>(array[x,0]);
                value = InterpreterHelper.CastValue<T2>(array[x, 1]);

                dictionary[key] = value;
            }

            return dictionary;
        }


        /// <summary>
        /// Set column names from an IDataRecord (DataReader is IDataRecord)
        /// Existing column names will be cleared
        /// </summary>
        /// <param name="array">Array to set</param>
        /// <param name="rec">The IDataRecord</param>
        public static void SetColumnNamesFrom(this FBasicArray array, IDataRecord rec)
        {
            array.ResetColumnNames();
            for (int inx = 0; inx < rec.FieldCount; inx++)
            {
                array.SetColumnName(1+inx,rec.GetName(inx)); // 1+ as the FBasic arrays are 1-based 
            }
        }

        /// <summary>
        /// Set column names from another FBasic Array
        /// Existing column names will be cleared
        /// </summary>
        /// <param name="array">Array to set</param>
        /// <param name="src">The source array</param>
        public static void SetColumnNamesFrom(this FBasicArray array, FBasicArray src)
        {
            array.ResetColumnNames();
            for (int inx = 0; inx < src.ColumnNamesCount; inx++)
            {
                array.SetColumnName(1 + inx, src.GetColumnName(1+inx)); // 1+ as the FBasic arrays are 1-based 
            }
        }

    }
}
