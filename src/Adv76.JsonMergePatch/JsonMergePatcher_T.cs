using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Adv76.JsonMergePatch;

public static partial class JsonMergePatcher
{
    public static void ApplyTo<T>(ref T obj, string patchString, JsonMergeOptions? mergeOptions = null)
    {
        var patchBytes = Encoding.UTF8.GetBytes(patchString);
        
        ApplyTo(ref obj, patchBytes, mergeOptions);
    }
    
    public static void ApplyTo<T>(ref T obj, byte[] patchBytes, JsonMergeOptions? mergeOptions = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(patchBytes);

        mergeOptions ??= JsonMergeOptions.Default;
        
        var jsonOptions = mergeOptions.JsonSerializerOptions ?? JsonSerializerOptions.Default;
        
        var reader = new Utf8JsonReader(patchBytes);

        try
        {
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var typeInfo = jsonOptions.GetTypeInfo(obj.GetType());
                
                ApplyToObject(ref reader, ref obj, typeInfo, [], mergeOptions, jsonOptions);
            }
            else
            {
                var converter = jsonOptions.GetConverter(typeof(T));

                var value = ReadValueWithConverter(ref reader, converter, typeof(T), jsonOptions);

                obj = (T)value!;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException or JsonMergePatchException)
        {
            throw new JsonMergePatchException("Json Merge Patch failed to apply.", e);
        }
    }

    private static void ApplyToObject<TInner>(ref Utf8JsonReader reader, ref TInner obj, JsonTypeInfo typeInfo, string[] path, JsonMergeOptions mergeOptions, JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(obj);
        
        reader.Read();

        // Read until there are no more properties left
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonMergePatchException("Invalid JSON. Expected property name.");
            }
                
            var propertyName = reader.GetString();
            if (propertyName is null)
            {
                throw new JsonMergePatchException($"Invalid JSON. Property name is null.");
            } 
            
            reader.Read();
                
            var jsonProperty = typeInfo.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (jsonProperty is null)
            {
                throw new JsonMergePatchException($"Property {propertyName} not found in type {typeInfo.Type}.");
            }

            if (jsonProperty.Set is null)
            {
                throw new JsonMergePatchException($"Property {propertyName} cannot be set in type {typeInfo.Type}.");
            }

            if (IsPropertyPatchable(jsonProperty, mergeOptions))
            {

                if (reader.TokenType == JsonTokenType.StartObject && !UseCustomConverterForObject(jsonProperty))
                {
                    var propertyObjectTypeInfo = jsonOptions.GetTypeInfo(jsonProperty.PropertyType);

                    var existing = jsonProperty.Get?.Invoke(obj);

                    if (existing is null)
                    {
                        var newValue = propertyObjectTypeInfo.CreateObject?.Invoke();

                        existing = newValue ??
                                   throw new JsonMergePatchException(
                                       $"Object type {jsonProperty.PropertyType.Name} cannot be created.");
                    }

                    ApplyToObject(ref reader, ref existing, propertyObjectTypeInfo, [..path, propertyName], mergeOptions, jsonOptions);

                    jsonProperty.Set(obj, existing);
                }
                else
                {
                    var converter = (jsonProperty.CustomConverter ??
                                     jsonOptions.GetConverter(jsonProperty.PropertyType));

                    try
                    {
                        var value = ReadValueWithConverter(ref reader, converter, jsonProperty.PropertyType, jsonOptions);
                        
                        if (jsonProperty.IsRequired && value is null)
                        {
                            throw new JsonMergePatchException(
                                $"Property {GetPropertyPath(path, propertyName)} is required and cannot be set to 'null'.");
                        }
                        
                        jsonProperty.Set(obj, value);
                    }
                    catch (Exception e)
                    {
                        _ = e;
                        throw;
                    }
                    
                }
            }
            else
            {
                // Skip the current node if object/array and move on
                reader.Skip();
            }

            reader.Read();
        }
    }

    private static string GetPropertyPath(string[] path, string propertyName)
    {
        return string.Join('.', [..path, propertyName]);
    }

    private static bool UseCustomConverterForObject(JsonPropertyInfo matchingProperty)
    {
        if (matchingProperty.AttributeProvider is null)
        {
            return false;
        }
        
        var attributes = matchingProperty.AttributeProvider.GetCustomAttributes(typeof(JsonMergeConverterBehaviorAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergeConverterBehaviorAttribute attribute)
        {
            return attribute.Behavior == JsonMergeConverterBehavior.UseCustomConverter;
        }

        return false;
    }

    private static bool IsPropertyPatchable(JsonPropertyInfo propertyInfo, JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return false;
        }
        
        var attributes = propertyInfo.AttributeProvider.GetCustomAttributes(typeof(JsonMergePropertySecurityAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergePropertySecurityAttribute attribute)
        {
            if (attribute.Policy == JsonMergePropertySecurityPolicy.BlockPatching)
            {
                throw new JsonMergePatchException($"Property {propertyInfo.Name} is cannot be patched.");
            }
            
            if (attribute.Policy == JsonMergePropertySecurityPolicy.AllowPatching)
            {
                return true;
            }

            // attribute.Policy == JsonMergePropertySecurityPolicy.SkipSilently
            // or any invalid enum states
            return false;
        }

        if (mergeOptions.SecurityPolicy == JsonMergeSecurityPolicy.BlockByDefault)
        {
            throw new JsonMergePatchException($"Property {propertyInfo.Name} is cannot be patched.");
        }

        return true;
    }

    private static object? ReadValueWithConverter(ref Utf8JsonReader reader, JsonConverter converter, Type propertyType,
        JsonSerializerOptions jsonOptions)
    {
        var readDelegate = CreateGenericReadDelegate(propertyType);
        
        return readDelegate(converter, ref reader, propertyType, jsonOptions);
    }
    
    private delegate object? ReadDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadDelegate CreateGenericReadDelegate(Type valueType)
    {
        var m = typeof(JsonMergePatcher).GetMethod(nameof(Read), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);

        return m.CreateDelegate<ReadDelegate>();
    }

    private static object? Read<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).Read(ref r, t, o);
}
