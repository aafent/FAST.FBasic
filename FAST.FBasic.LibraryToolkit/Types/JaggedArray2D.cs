
namespace FAST.FBasicInterpreter.Types
{
    /// <summary>
    /// Generic 2D Jagged Array
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        public int Length => m_values?.Length ?? 0;

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
            if (m_values.Length < 1) return;
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
            m_values = null;
        }
    }


}
