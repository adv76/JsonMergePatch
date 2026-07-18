using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Adv76.JsonMergePatch;

public class JsonMergePatch<T>
{
    private readonly JsonNode _patchDocument;
    private readonly string _patchString;

    private readonly JsonSerializerOptions _options = JsonSerializerOptions.Default;
    
    public JsonMergePatch(JsonNode patchDocument)
    {
        _patchDocument = patchDocument;   
    }
    
    public JsonMergePatch(string patchString)
    {
        _patchString = patchString; 
    }

    public void ApplyTo(ref T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        
        var bytes = Encoding.UTF8.GetBytes(_patchString);
        var reader = new Utf8JsonReader(bytes);
        
        reader.Read();
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var typeInfo = _options.GetTypeInfo(obj.GetType());

            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonMergePatchException("Invalid JSON. Expected property name.");
                }
                
                var propertyName = reader.GetString();
                reader.Read(); // skip end of property name
                
                //if (reader.TokenType != JsonTokenType.PropertyName)
                //{
                //    throw new JsonMergePatchException("Invalid JSON. Expected end of property name.");
                //}
                
                var matchingProperty = typeInfo.Properties.FirstOrDefault(x => x.Name == propertyName);
                if (matchingProperty is null)
                {
                    throw new JsonMergePatchException($"Property {propertyName} not found in type {typeInfo.Type}");
                }

                if (matchingProperty.Set is null)
                {
                    throw new JsonMergePatchException($"Property {propertyName} cannot be set in type {typeInfo.Type}");
                }
                
                var converter = (matchingProperty.CustomConverter ?? _options.GetConverter(matchingProperty.PropertyType));
                
                var read = Create(matchingProperty.PropertyType);
                var value = read(converter, ref reader, matchingProperty.PropertyType, _options);
                //var value = converter.Read(ref reader, matchingProperty.PropertyType, _options);
                
                matchingProperty.Set(obj, value);
                
                reader.Read();
            }
        }
        
        
        
    }
    
    private delegate object? ReadDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadDelegate Create(Type valueType)
    {
        var m = typeof(JsonMergePatch<object>).GetMethod(nameof(Read), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);

        return m.CreateDelegate<ReadDelegate>();
    }

    private static object? Read<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).Read(ref r, t, o);
}