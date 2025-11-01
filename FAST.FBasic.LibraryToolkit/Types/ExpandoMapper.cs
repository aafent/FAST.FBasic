using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace FAST.FBasicInterpreter.Types
{
    /// <summary>
    /// Key Features:
    /// Generic method MapToType<T>() and non-generic MapToType() for flexibility
    /// Deep cloning - recursively maps nested objects and collections at any depth
    /// Array support - handles arrays of any type including nested objects
    /// Collection support - works with List<T>, IList<T>, ICollection<T>, IEnumerable<T>
    /// Record support - properly handles C# records with primary constructors and init-only properties
    /// Case-insensitive matching - property names are matched regardless of casing
    /// Graceful bypassing - ignores properties that don't exist or don't match in either the source or target
    /// Type conversion - automatically converts compatible types (primitives, DateTime, Guid, etc.)
    /// 
    /// How it works:
    /// Creates an instance of the target type (handles parameterless and parameterized constructors)
    /// Iterates through all writable properties
    /// Matches properties by name (case-insensitive)
    /// Recursively maps nested ExpandoObjects, arrays, and collections
    /// Silently skips properties that can't be mapped
    /// The mapper handles complex scenarios like records with primary constructors, nested objects at any depth, and mixed collections of primitives and complex types.
    /// </summary>
    public static class ExpandoMapper
    {
        /// <summary>
        /// Maps an ExpandoObject to a specified type or record, copying matching properties recursively.
        /// </summary>
        /// <typeparam name="T">The target type to map to</typeparam>
        /// <param name="expandoObject">The source ExpandoObject</param>
        /// <returns>An instance of T with mapped properties</returns>
        public static T MapToType<T>(ExpandoObject expandoObject)
        {
            return (T)MapToType(expandoObject, typeof(T));
        }

        /// <summary>
        /// Maps an ExpandoObject to a specified type or record, copying matching properties recursively.
        /// </summary>
        /// <param name="expandoObject">The source ExpandoObject</param>
        /// <param name="targetType">The target type to map to</param>
        /// <returns>An instance of the target type with mapped properties</returns>
        public static object MapToType(ExpandoObject expandoObject, Type targetType)
        {
            if (expandoObject == null)
                return null!;

            // Get the dictionary representation of the ExpandoObject
            var expandoDict = (IDictionary<string, object>)expandoObject;

            // Create an instance of the target type
            object instance = CreateInstance(targetType, expandoDict);

            // Get all writable properties of the target type
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite || IsInitOnlyProperty(p));

            foreach (var property in properties)
            {
                // Try to find a matching key in the expando object (case-insensitive)
                var matchingKey = expandoDict.Keys.FirstOrDefault(k =>
                    string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));

                if (matchingKey == null)
                    continue;

                var expandoValue = expandoDict[matchingKey];

                if (expandoValue == null)
                {
                    SetPropertyValue(instance, property, null);
                    continue;
                }

                try
                {
                    var mappedValue = MapValue(expandoValue, property.PropertyType);
                    SetPropertyValue(instance, property, mappedValue);
                }
                catch
                {
                    // Bypass if mapping fails
                    continue;
                }
            }

            return instance;
        }


        private static object MapValue(object value, Type targetType)
        {
            if (value == null)
                return null;


            if (value is JsonElement)
            {
                value = JsonElementConverter.ConvertToPlainValue((JsonElement)value);
            }

            var valueType = value.GetType();

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
                targetType = underlyingType;

            // Direct assignment for matching types or convertible types
            if (targetType.IsAssignableFrom(valueType))
                return value;

            // Handle primitive types and strings
            if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal) ||
                targetType == typeof(DateTime) || targetType == typeof(Guid))
            {
                return Convert.ChangeType(value, targetType);
            }

            // Handle arrays
            if (targetType.IsArray)
            {
                return MapArray(value, targetType);
            }

            // Handle generic lists and collections
            if (targetType.IsGenericType)
            {
                var genericTypeDef = targetType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(List<>) ||
                    genericTypeDef == typeof(IList<>) ||
                    genericTypeDef == typeof(ICollection<>) ||
                    genericTypeDef == typeof(IEnumerable<>))
                {
                    return MapList(value, targetType);
                }
            }

            // Handle nested ExpandoObjects
            if (value is ExpandoObject expandoValue)
            {
                return MapToType(expandoValue, targetType);
            }

            // Handle dictionaries as ExpandoObjects
            if (value is IDictionary<string, object> dict)
            {
                var expando = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)expando;
                foreach (var kvp in dict)
                {
                    expandoDict[kvp.Key] = kvp.Value;
                }
                return MapToType(expando, targetType);
            }

            // Try to convert as a last resort
            return Convert.ChangeType(value, targetType);
        }

        private static object MapArray(object value, Type targetType)
        {
            var elementType = targetType.GetElementType();

            if (value is IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                var array = Array.CreateInstance(elementType, items.Count);

                for (int i = 0; i < items.Count; i++)
                {
                    var mappedItem = MapValue(items[i], elementType);
                    array.SetValue(mappedItem, i);
                }

                return array;
            }

            throw new InvalidOperationException("Cannot map non-enumerable to array");
        }

        private static object MapList(object value, Type targetType)
        {
            var elementType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType);

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var mappedItem = MapValue(item, elementType);
                    list.Add(mappedItem);
                }
            }

            return list;
        }

        private static object CreateInstance(Type type, IDictionary<string, object> expandoDict)
        {
            // For records and classes with primary constructors, try to match constructor parameters
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length);

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0)
                {
                    return Activator.CreateInstance(type);
                }

                // Try to match all constructor parameters with expando properties
                var paramValues = new object[parameters.Length];
                var allMatched = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var matchingKey = expandoDict.Keys.FirstOrDefault(k =>
                        string.Equals(k, param.Name, StringComparison.OrdinalIgnoreCase));

                    if (matchingKey != null)
                    {
                        try
                        {
                            paramValues[i] = MapValue(expandoDict[matchingKey], param.ParameterType);
                        }
                        catch
                        {
                            paramValues[i] = GetDefault(param.ParameterType);
                        }
                    }
                    else
                    {
                        paramValues[i] = GetDefault(param.ParameterType);
                    }
                }

                try
                {
                    return Activator.CreateInstance(type, paramValues);
                }
                catch
                {
                    continue;
                }
            }

            // Fallback to parameterless constructor
            return Activator.CreateInstance(type);
        }

        private static void SetPropertyValue(object instance, PropertyInfo property, object value)
        {
            if (property.CanWrite)
            {
                property.SetValue(instance, value);
            }
            else if (IsInitOnlyProperty(property))
            {
                // Handle init-only properties (records)
                property.SetValue(instance, value);
            }
        }

        private static bool IsInitOnlyProperty(PropertyInfo property)
        {
            return property.SetMethod?.ReturnParameter
                ?.GetRequiredCustomModifiers()
                ?.Any(t => t.Name == "IsExternalInit") ?? false;
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}