using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DynamicPropertyAccess
{
    /// <summary>
    /// Exception thrown when a requested index is out of bounds in a collection
    /// </summary>
    public class RequestedIndexNotExistsException : Exception
    {
        public int RequestedIndex { get; }
        public int CollectionSize { get; }
        public string PropertyPath { get; }

        public RequestedIndexNotExistsException(int requestedIndex, int collectionSize, string propertyPath)
            : base($"Requested index [{requestedIndex}] does not exist. Collection size is {collectionSize}. Path: {propertyPath}")
        {
            RequestedIndex = requestedIndex;
            CollectionSize = collectionSize;
            PropertyPath = propertyPath;
        }
    }

    public static class ObjectNavigator
    {
        /// <summary>
        /// Gets the Type of the value at the specified path expression
        /// </summary>
        /// <param name="instance">The root object to navigate from</param>
        /// <param name="expression">Path expression (e.g., "field1.field2.addresses[1].city")</param>
        /// <returns>The Type of the value at the specified path</returns>
        public static Type GetValueType(object instance, string expression)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

            var segments = ParseExpression(expression);
            Type currentType = instance.GetType();
            string currentPath = "";

            foreach (var segment in segments)
            {
                currentPath += string.IsNullOrEmpty(currentPath) ? segment.Name : $".{segment.Name}";

                // Get the property or field type
                currentType = GetPropertyOrFieldType(currentType, segment.Name, currentPath);

                // If indexed, get the element type
                if (segment.IsIndexed)
                {
                    currentType = GetElementType(currentType, currentPath);
                    currentPath += $"[{segment.Index}]";
                }
            }

            return currentType;
        }

        /// <summary>
        /// Navigates through an object hierarchy using a dot-notation path expression
        /// </summary>
        /// <param name="instance">The root object to navigate from</param>
        /// <param name="expression">Path expression (e.g., "field1.field2.addresses[1].city")</param>
        /// <returns>The value at the specified path, or null if any intermediate value is null</returns>
        public static object? GetValue(object instance, string expression)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

            var segments = ParseExpression(expression);
            object? current = instance;
            string currentPath = "";

            foreach (var segment in segments)
            {
                if (current == null)
                    return null;

                currentPath += string.IsNullOrEmpty(currentPath) ? segment.Name : $".{segment.Name}";

                if (segment.IsIndexed)
                {
                    current = GetIndexedValue(current, segment.Index!.Value, currentPath);
                    currentPath += $"[{segment.Index}]";
                }
                else
                {
                    current = GetPropertyOrFieldValue(current, segment.Name, currentPath);
                }
            }

            return current;
        }

        private static Type GetPropertyOrFieldType(Type type, string name, string path)
        {
            // Try property first
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.PropertyType;
            }

            // Try field
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                return field.FieldType;
            }

            throw new InvalidOperationException($"Property or field '{name}' not found on type '{type.Name}' at path '{path}'");
        }

        private static Type GetElementType(Type collectionType, string path)
        {
            // Handle arrays
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType()!;
            }

            // Handle generic collections (List<T>, IEnumerable<T>, etc.)
            if (collectionType.IsGenericType)
            {
                var genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    return genericArgs[0];
                }
            }

            // Handle IEnumerable interface on generic types
            var enumerableInterface = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                return enumerableInterface.GetGenericArguments()[0];
            }

            // Fallback to object for non-generic IEnumerable
            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                return typeof(object);
            }

            throw new InvalidOperationException($"Cannot determine element type for collection type '{collectionType.Name}' at path '{path}'");
        }

        private static object? GetPropertyOrFieldValue(object obj, string name, string path)
        {
            var type = obj.GetType();

            // Try property first
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.GetValue(obj);
            }

            // Try field
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            throw new InvalidOperationException($"Property or field '{name}' not found on type '{type.Name}' at path '{path}'");
        }

        private static object? GetIndexedValue(object obj, int index, string path)
        {
            // Handle arrays
            if (obj is Array array)
            {
                if (index < 0 || index >= array.Length)
                {
                    throw new RequestedIndexNotExistsException(index, array.Length, path);
                }
                return array.GetValue(index);
            }

            // Handle IList (List<T>, etc.)
            if (obj is IList list)
            {
                if (index < 0 || index >= list.Count)
                {
                    throw new RequestedIndexNotExistsException(index, list.Count, path);
                }
                return list[index];
            }

            // Handle IEnumerable as fallback
            if (obj is IEnumerable enumerable)
            {
                var enumerableList = enumerable.Cast<object>().ToList();
                if (index < 0 || index >= enumerableList.Count)
                {
                    throw new RequestedIndexNotExistsException(index, enumerableList.Count, path);
                }
                return enumerableList[index];
            }

            throw new InvalidOperationException($"Cannot apply indexer to type '{obj.GetType().Name}' at path '{path}'");
        }

        private static PathSegment[] ParseExpression(string expression)
        {
            // Pattern to match: propertyName or propertyName[index]
            var pattern = @"([a-zA-Z_][a-zA-Z0-9_]*)(?:\[(\d+)\])?";
            var matches = Regex.Matches(expression.Replace(" ", ""), pattern);

            var segments = new System.Collections.Generic.List<PathSegment>();

            foreach (Match match in matches)
            {
                if (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    var name = match.Groups[1].Value;
                    int? index = null;

                    if (match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        index = int.Parse(match.Groups[2].Value);
                    }

                    segments.Add(new PathSegment(name, index));
                }
            }

            if (segments.Count == 0)
            {
                throw new ArgumentException($"Invalid expression format: '{expression}'");
            }

            return segments.ToArray();
        }

        private class PathSegment
        {
            public string Name { get; }
            public int? Index { get; }
            public bool IsIndexed => Index.HasValue;

            public PathSegment(string name, int? index)
            {
                Name = name;
                Index = index;
            }

            public override string ToString()
            {
                return IsIndexed ? $"{Name}[{Index}]" : Name;
            }
        }
    }
}

// Example usage:
/*
class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

class Person
{
    public string Name { get; set; }
    public Address[] Addresses { get; set; }
}

class Company
{
    public Person[] Employees { get; set; }
}

// Usage:
var company = new Company
{
    Employees = new[]
    {
        new Person
        {
            Name = "John",
            Addresses = new[]
            {
                new Address { City = "New York", Street = "5th Ave" },
                new Address { City = "Boston", Street = "Main St" }
            }
        }
    }
};

// Get value
var city = ObjectNavigator.GetValue(company, "Employees[0].Addresses[1].City");
// Returns: "Boston"

// Get type
var cityType = ObjectNavigator.GetValueType(company, "Employees[0].Addresses[1].City");
// Returns: typeof(string)

var addressType = ObjectNavigator.GetValueType(company, "Employees[0].Addresses[1]");
// Returns: typeof(Address)

var nameType = ObjectNavigator.GetValueType(company, "Employees[0].Name");
// Returns: typeof(string)

try
{
    var invalid = ObjectNavigator.GetValue(company, "Employees[0].Addresses[5].City");
}
catch (RequestedIndexNotExistsException ex)
{
    Console.WriteLine(ex.Message);
    // Output: Requested index [5] does not exist. Collection size is 2. Path: Employees[0].Addresses
}
*/





/* PROMPT:
 * 

You are an expert C#, net8 developer. I want a method that will take at least 2 arguments. First will be the instance of an object and will be type of Object. The second will be an expression of type string. The expression will contain a notation similar to c# notation to identify fields, array items, subclasses etc. for example "field1.field2.addresses[1].city" should return from the subclass of the field1 then the subclass field2  where have an array with name addresses the 1st item of the array, the field city. ask me if you need clarifications

the array indexing will be only numeric. if a value in the tree is null will return null, no matter if the notation have more brances. the return will be of type object? always.    yes will support all collections. Yes if something does not exists, will raise an error. Especially for the array index not exists error, when an given index is out of the instance bounds will return a special execption the: RequestedIndexNotExistsExecption



add a method that will take as argument one of the patterns that the class handles and will return the type of it.  patterns example: "field1.field2"
* "addresses[1].city"
* "employees[0].addresses[1].street"
* "data.items[5].nested.value"




 * 
 */