namespace JaggedSortDemo
{
    /// <summary>
    /// Provides a static method for sorting jagged arrays (T[][])
    /// based on dynamic, multi-column sort criteria.
    /// This implementation targets .NET 8.
    /// </summary>
    public static class JaggedArraySorter
    {
        /// <summary>
        /// Sorts a jagged array (array of arrays) in place based on multiple column criteria.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array. Must implement IComparable<T>.</typeparam>
        /// <param name="array">The jagged array (T[][]) to sort. This array is sorted in place.</param>
        /// <param name="sortDirections">
        /// An array of strings ("A" for Ascending, "D" for Descending).
        /// The position in this array corresponds to the column index (e.g., sortDirections[0] is for column 0).
        /// The sort is stable, applying criteria in the order they appear in this array.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if array or sortDirections is null.</exception>
        /// <exception cref="ArgumentException">Thrown if sortDirections contains invalid values (not "A" or "D").</exception>
        public static void SortJaggedArray<T>(T[][] array, string[] sortDirections) where T : IComparable<T>
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (sortDirections == null)
                throw new ArgumentNullException(nameof(sortDirections));
            if (sortDirections.Length == 0)
                return; // Nothing to sort by

            // --- Validation Step ---
            // Pre-validate all sort directions to fail fast before sorting.
            for (int i = 0; i < sortDirections.Length; i++)
            {
                if (string.IsNullOrEmpty(sortDirections[i]) ||
                    (!sortDirections[i].Equals("A", StringComparison.OrdinalIgnoreCase) &&
                     !sortDirections[i].Equals("D", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Invalid sort direction at index {i}: '{sortDirections[i]}'. Must be 'A' or 'D'.");
                }
            }

            // --- Sorting Step ---
            // Use Array.Sort with a custom Comparison<T[]> delegate.
            // This delegate implements the multi-column comparison logic.
            Array.Sort(array, (rowA, rowB) =>
            {
                // Handle null rows. We'll sort them to the beginning.
                if (rowA == null && rowB == null) return 0; // Both are null
                if (rowA == null) return -1; // rowA is null, rowB is not. rowA comes first.
                if (rowB == null) return 1;  // rowB is null, rowA is not. rowB comes first.

                // Iterate through each sort criterion
                for (int i = 0; i < sortDirections.Length; i++)
                {
                    bool isAscending = sortDirections[i].Equals("A", StringComparison.OrdinalIgnoreCase);

                    bool rowAHasValue = rowA.Length > i;
                    bool rowBHasValue = rowB.Length > i;

                    // Case 1: Both rows are too short for this column index.
                    // They are equal for this criterion, so move to the next.
                    if (!rowAHasValue && !rowBHasValue)
                    {
                        continue;
                    }

                    // Case 2: One row is short, the other is not.
                    // The short row is treated as "smaller" in ASC, "larger" in DESC.
                    if (!rowAHasValue) // rowA is short
                    {
                        return isAscending ? -1 : 1;
                    }
                    if (!rowBHasValue) // rowB is short
                    {
                        return isAscending ? 1 : -1;
                    }

                    // Case 3: Both rows have a value at this index.
                    T valueA = rowA[i];
                    T valueB = rowB[i];

                    int comparison;

                    // Handle null values *within* the rows (e.g., string[i] is null)
                    if (valueA == null && valueB == null)
                    {
                        comparison = 0; // Both are null, equal for this criterion
                    }
                    else if (valueA == null) // valueA is null
                    {
                        // Nulls are "smaller" in ASC, "larger" in DESC
                        comparison = isAscending ? -1 : 1;
                    }
                    else if (valueB == null) // valueB is null
                    {
                        comparison = isAscending ? 1 : -1;
                    }
                    else
                    {
                        // Case 4: Neither value is null, use default comparison.
                        comparison = valueA.CompareTo(valueB);
                    }

                    // If a difference is found, return the result
                    if (comparison != 0)
                    {
                        // For ascending, return the comparison.
                        // For descending, flip the sign of the comparison.
                        return isAscending ? comparison : -comparison;
                    }
                }

                // If after checking all criteria, the rows are still equal,
                // return 0 to maintain a stable sort.
                return 0;
            });
        }
    }
}
