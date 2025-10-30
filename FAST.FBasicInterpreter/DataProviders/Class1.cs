using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

public class FBasicDynamicObject : DynamicObject
{
    private readonly Dictionary<string, object?> _properties = new();

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        return _properties.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _properties[binder.Name] = value;
        return true;
    }

    public void AddProperty(string name, object? value)
    {
        _properties[name] = value;
    }

    public Dictionary<string, object?> GetProperties()
    {
        return _properties;
    }
}

public static class FBasicDynamicJsonDeserializer
{
    /// <summary>
    /// Deserializes JSON string to a dynamic object with property access (obj.Property)
    /// </summary>
    public static dynamic Deserialize(string json)
    {
        var jsonNode = JsonNode.Parse(json);
        return ConvertJsonNodeToDynamic(jsonNode);
    }

    /// <summary>
    /// Serializes the dynamic object back to JSON string
    /// </summary>
    public static string Serialize(dynamic obj, bool indented = true)
    {
        var jsonNode = ConvertDynamicToJsonNode(obj);
        return jsonNode?.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = indented
        }) ?? "null";
    }

    private static object? ConvertJsonNodeToDynamic(JsonNode? node)
    {
        if (node == null)
            return null;

        switch (node)
        {
            case JsonObject jsonObject:
                var dynamicObj = new FBasicDynamicObject();
                foreach (var property in jsonObject)
                {
                    dynamicObj.AddProperty(property.Key, ConvertJsonNodeToDynamic(property.Value));
                }
                return dynamicObj;

            case JsonArray jsonArray:
                var list = new List<object?>();
                foreach (var item in jsonArray)
                {
                    list.Add(ConvertJsonNodeToDynamic(item));
                }
                return list;

            case JsonValue jsonValue:
                return GetJsonValueAsObject(jsonValue);

            default:
                return node.ToJsonString();
        }
    }

    private static object? GetJsonValueAsObject(JsonValue jsonValue)
    {
        if (jsonValue.TryGetValue<string>(out var stringValue))
            return stringValue;
        if (jsonValue.TryGetValue<long>(out var longValue))
            return longValue;
        if (jsonValue.TryGetValue<int>(out var intValue))
            return intValue;
        if (jsonValue.TryGetValue<double>(out var doubleValue))
            return doubleValue;
        if (jsonValue.TryGetValue<bool>(out var boolValue))
            return boolValue;
        if (jsonValue.TryGetValue<decimal>(out var decimalValue))
            return decimalValue;

        return jsonValue.ToJsonString();
    }

    private static JsonNode? ConvertDynamicToJsonNode(object? obj)
    {
        if (obj == null)
            return null;

        if (obj is FBasicDynamicObject dynamicObj)
        {
            var jsonObject = new JsonObject();
            foreach (var property in dynamicObj.GetProperties())
            {
                jsonObject[property.Key] = ConvertDynamicToJsonNode(property.Value);
            }
            return jsonObject;
        }

        if (obj is IList<object?> list)
        {
            var jsonArray = new JsonArray();
            foreach (var item in list)
            {
                jsonArray.Add(ConvertDynamicToJsonNode(item));
            }
            return jsonArray;
        }

        // Handle primitive types
        return obj switch
        {
            string s => JsonValue.Create(s),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            double d => JsonValue.Create(d),
            float f => JsonValue.Create(f),
            decimal dec => JsonValue.Create(dec),
            bool b => JsonValue.Create(b),
            _ => JsonValue.Create(obj.ToString())
        };
    }
}

// Usage Examples
public class Program
{
    public static void Main()
    {
        string json = @"{
            ""name"": ""John Doe"",
            ""age"": 30,
            ""salary"": 75000.50,
            ""isActive"": true,
            ""email"": ""john@example.com"",
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""New York"",
                ""zipCode"": ""10001""
            },
            ""hobbies"": [""reading"", ""gaming"", ""coding""],
            ""scores"": [95, 87, 92, 88],
            ""projects"": [
                {
                    ""name"": ""Project A"",
                    ""status"": ""completed""
                },
                {
                    ""name"": ""Project B"",
                    ""status"": ""in-progress""
                }
            ]
        }";

        Console.WriteLine("=== Deserializing JSON ===\n");

        // Deserialize
        dynamic person = FBasicDynamicJsonDeserializer.Deserialize(json);

        // Access properties with dot notation
        Console.WriteLine($"Name: {person.name}");
        Console.WriteLine($"Age: {person.age}");
        Console.WriteLine($"Salary: {person.salary}");
        Console.WriteLine($"Is Active: {person.isActive}");
        Console.WriteLine($"Email: {person.email}");

        // Access nested object properties
        Console.WriteLine($"\nAddress:");
        Console.WriteLine($"  Street: {person.address.street}");
        Console.WriteLine($"  City: {person.address.city}");
        Console.WriteLine($"  Zip: {person.address.zipCode}");

        // Access array of strings
        Console.WriteLine($"\nHobbies:");
        foreach (var hobby in person.hobbies)
        {
            Console.WriteLine($"  - {hobby}");
        }

        // Access array of numbers
        Console.WriteLine($"\nScores:");
        foreach (var score in person.scores)
        {
            Console.WriteLine($"  - {score}");
        }

        // Access array of objects
        Console.WriteLine($"\nProjects:");
        foreach (var project in person.projects)
        {
            Console.WriteLine($"  - {project.name}: {project.status}");
        }

        // Modify properties
        Console.WriteLine("\n=== Modifying Properties ===\n");
        person.age = 31;
        person.address.city = "Los Angeles";
        person.newProperty = "This was added dynamically";

        Console.WriteLine($"Updated Age: {person.age}");
        Console.WriteLine($"Updated City: {person.address.city}");
        Console.WriteLine($"New Property: {person.newProperty}");

        // Serialize back to JSON
        Console.WriteLine("\n=== Serialized Back to JSON ===\n");
        string serialized = FBasicDynamicJsonDeserializer.Serialize(person);
        Console.WriteLine(serialized);

        // Verify round-trip
        Console.WriteLine("\n=== Verify Round-Trip ===\n");
        dynamic person2 = FBasicDynamicJsonDeserializer.Deserialize(serialized);
        Console.WriteLine($"Name after round-trip: {person2.name}");
        Console.WriteLine($"Age after round-trip: {person2.age}");
        Console.WriteLine($"City after round-trip: {person2.address.city}");
        Console.WriteLine($"New Property after round-trip: {person2.newProperty}");
    }
}