using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace FAST.FBasicInterpreter.Types
{
    /// <summary>
    /// Provides methods for converting complex object graphs into nested ExpandoObjects.
    /// </summary>
    public static class ExpandoConverter
    {
        /// <summary>
        /// Recursively converts any object (including its properties, nested objects, and collections)
        /// into a dynamic ExpandoObject structure.
        /// </summary>
        /// <param name="obj">The source object to convert (can be dynamic, anonymous, or a defined class).</param>
        /// <returns>A dynamic object graph composed of ExpandoObjects, Lists, and primitive types.</returns>
        public static dynamic ToDeepExpando(object obj)
        {
            // 1. Handle Null: The conversion stops here.
            if (obj == null)
            {
                return null;
            }

            // 2. Stop condition for Primitives/Value Types/Strings/Already Expando:
            // These types are returned directly as they cannot be further converted.
            var type = obj.GetType();
            if (obj is ExpandoObject || type.IsPrimitive || type.IsEnum || obj is string || obj is decimal)
            {
                return obj;
            }

            // 3. Handle Collections (Arrays, Lists, etc.)
            if (obj is IEnumerable nonStringEnumerable)
            {
                // Note: Dictionaries (which are IEnumerables) will be treated as collections of KeyValuePairs.
                // If specific Dictionary behavior is needed, add an explicit check for IDictionary here.

                var convertedList = new List<dynamic>();
                foreach (var item in nonStringEnumerable)
                {
                    // Recursively convert each item in the collection
                    convertedList.Add(ToDeepExpando(item));
                }
                return convertedList;
            }

            // 4. Handle Complex Objects (POCOs, Anonymous Types)
            IDictionary<string, object> expando = new ExpandoObject();

            // Get all public instance properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Skip properties that cannot be read
                if (!property.CanRead)
                {
                    continue;
                }

                try
                {
                    var value = property.GetValue(obj);

                    // Recursively convert the property value
                    expando.Add(property.Name, ToDeepExpando(value));
                }
                catch (Exception ex)
                {
                    // Log reflection errors if necessary, but continue processing other properties.
                    Console.WriteLine($"Warning: Could not get value for property '{property.Name}'. Error: {ex.Message}");
                    expando.Add(property.Name, null);
                }
            }

            return (ExpandoObject)expando;
        }
    }

}
