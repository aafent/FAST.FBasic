using System.Text.Json;

namespace FAST.FBasicInterpreter.Types
{
    public static class JsonElementConverter
    {
        /// <summary>
        /// Converts a JsonElement to its corresponding plain .NET type based on ValueKind
        /// </summary>
        /// <param name="element">The JsonElement to convert</param>
        /// <returns>The converted value as object, or null for Null/Undefined kinds</returns>
        public static object? ConvertToPlainValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.TryGetInt64(out long longValue)
                    ? longValue
                    : element.GetDouble(),

                JsonValueKind.String => element.GetString(),

                JsonValueKind.True => true,

                JsonValueKind.False => false,

                JsonValueKind.Null => null,

                JsonValueKind.Undefined => null,

                JsonValueKind.Object => ConvertObjectToDictionary(element),

                JsonValueKind.Array => ConvertArrayToList(element),

                _ => throw new ArgumentException($"Unknown JsonValueKind: {element.ValueKind}")
            };
        }

        /// <summary>
        /// Converts a JsonElement object to a Dictionary with string keys and object values
        /// </summary>
        private static Dictionary<string, object?> ConvertObjectToDictionary(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Element must be of type Object");
            }

            var dictionary = new Dictionary<string, object?>();

            foreach (var property in element.EnumerateObject())
            {
                dictionary[property.Name] = ConvertToPlainValue(property.Value);
            }

            return dictionary;
        }

        /// <summary>
        /// Converts a JsonElement array to a List of objects
        /// </summary>
        private static List<object?> ConvertArrayToList(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("Element must be of type Array");
            }

            var list = new List<object?>();

            foreach (var item in element.EnumerateArray())
            {
                list.Add(ConvertToPlainValue(item));
            }

            return list;
        }

        /// <summary>
        /// Converts a JsonElement to a specific type T
        /// </summary>
        public static T? ConvertTo<T>(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            {
                return default;
            }

            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle numbers
            if (element.ValueKind == JsonValueKind.Number)
            {
                if (underlyingType == typeof(double))
                    return (T)(object)element.GetDouble();
                if (underlyingType == typeof(float))
                    return (T)(object)element.GetSingle();
                if (underlyingType == typeof(decimal))
                    return (T)(object)element.GetDecimal();
                if (underlyingType == typeof(int))
                    return (T)(object)element.GetInt32();
                if (underlyingType == typeof(long))
                    return (T)(object)element.GetInt64();
                if (underlyingType == typeof(short))
                    return (T)(object)element.GetInt16();
                if (underlyingType == typeof(byte))
                    return (T)(object)element.GetByte();
                if (underlyingType == typeof(uint))
                    return (T)(object)element.GetUInt32();
                if (underlyingType == typeof(ulong))
                    return (T)(object)element.GetUInt64();
                if (underlyingType == typeof(ushort))
                    return (T)(object)element.GetUInt16();
            }

            // Handle other types
            if (underlyingType == typeof(string))
                return (T)(object)element.GetString()!;
            if (underlyingType == typeof(bool))
                return (T)(object)element.GetBoolean();
            if (underlyingType == typeof(DateTime))
                return (T)(object)element.GetDateTime();
            if (underlyingType == typeof(DateTimeOffset))
                return (T)(object)element.GetDateTimeOffset();
            if (underlyingType == typeof(Guid))
                return (T)(object)element.GetGuid();

            // Handle arrays and objects by deserializing to the target type
            if (element.ValueKind == JsonValueKind.Array || element.ValueKind == JsonValueKind.Object)
            {
                return element.Deserialize<T>();
            }

            throw new InvalidOperationException($"Cannot convert JsonElement to type {typeof(T).Name}");
        }
    }
}