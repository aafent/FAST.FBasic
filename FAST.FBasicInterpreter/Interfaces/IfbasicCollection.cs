using System.Collections;

namespace FAST.FBasicInterpreter
{
    /// <summary>
    /// Interface for FBASIC collections
    /// </summary>
    public interface IfbasicCollection : IEnumerator
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

    }
}
