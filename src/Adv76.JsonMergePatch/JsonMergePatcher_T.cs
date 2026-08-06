using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Adv76.JsonMergePatch;

namespace Adv76.JsonMergePatch;

public static partial class JsonMergePatcher
{
    public static void ApplyTo<T>(ref T obj, string patchString, JsonSerializerOptions? jsonOptions = null)
    {
        var patchBytes = Encoding.UTF8.GetBytes(patchString);
        
        ApplyTo(ref obj, patchBytes, jsonOptions);
    }
    
    public static void ApplyTo<T>(ref T obj, byte[] patchBytes, JsonSerializerOptions? jsonOptions = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(patchBytes);
        
        jsonOptions ??= JsonSerializerOptions.Default;
        
        var reader = new Utf8JsonReader(patchBytes);

        try
        {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                ApplyToObject(ref reader, ref obj, jsonOptions);
            }
            else
            {
                var converter = jsonOptions.GetConverter(typeof(T));

                var read = Create(typeof(T));
                var value = read(converter, ref reader, typeof(T), jsonOptions);

                obj = (T)value!;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException or JsonMergePatchException)
        {
            throw new JsonMergePatchException("Json Merge Patch failed to apply.", e);
        }
    }

    private static void ApplyToObject<TInner>(ref Utf8JsonReader reader, ref TInner obj, JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(obj);
        
        var typeInfo = jsonOptions.GetTypeInfo(obj.GetType());

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonMergePatchException("Invalid JSON. Expected property name.");
            }
                
            var propertyName = reader.GetString();
                
            reader.Read();
                
            var matchingProperty = typeInfo.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (matchingProperty is null)
            {
                throw new JsonMergePatchException($"Property {propertyName} not found in type {typeInfo.Type}.");
            }

            if (matchingProperty.Set is null)
            {
                throw new JsonMergePatchException($"Property {propertyName} cannot be set in type {typeInfo.Type}.");
            }
                
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var existing = matchingProperty.Get?.Invoke(obj) ?? Activator.CreateInstance(matchingProperty.PropertyType);

                ApplyToObject(ref reader, ref existing, jsonOptions);
                
                matchingProperty.Set(obj, existing);
            }
            else
            {
                var converter = (matchingProperty.CustomConverter ??
                                 jsonOptions.GetConverter(matchingProperty.PropertyType));

                var read = Create(matchingProperty.PropertyType);
                var value = read(converter, ref reader, matchingProperty.PropertyType, jsonOptions);

                if (matchingProperty.IsRequired && value is null)
                {
                    throw new JsonMergePatchException($"Property {propertyName} is required and cannot be set to 'null'.");
                }
                
                matchingProperty.Set(obj, value);
            }
                
            reader.Read();
        }
    }
    
    private delegate object? ReadDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadDelegate Create(Type valueType)
    {
        var m = typeof(JsonMergePatcher).GetMethod(nameof(Read), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);

        return m.CreateDelegate<ReadDelegate>();
    }

    private static object? Read<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).Read(ref r, t, o);
}