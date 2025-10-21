using System.Collections;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Interface for FBASIC collections
    /// </summary>
    public interface IBasicCollection : IEnumerator
    {
        /// <summary>
        /// True when the name reach the end of the data
        /// </summary>
        bool endOfData { get; set; }

        /// <summary>
        /// Get the value of a given name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The Value</returns>
        Value getValue(string name);

        /// <summary>
        /// This method used to clear (empty) the collection
        /// It is not the same with the Reset() from IEnumerator that it is used to jump to the first of the items.
        /// </summary>
        void ClearCollection();

    }
}
