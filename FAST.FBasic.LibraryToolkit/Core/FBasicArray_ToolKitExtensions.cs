namespace FAST.FBasicInterpreter
{
    public static class FBasicArray_ToolKitExtensions
    {

        public static Dictionary<T1, T2> ConvertToDictionary<T1,T2>(this FBasicArray array, int keyIndex=0, int valueIndex=1)
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

    }
}
