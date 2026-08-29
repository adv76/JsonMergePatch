using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Adv76.JsonMergePatch;

public static class JsonMergePatcher
{
    public static void ApplyTo<T>(ref T obj, byte[] patchBytes, JsonMergeOptions? mergeOptions = null)
    {
        var result = SafeApplyTo<T>(ref obj, patchBytes, mergeOptions);
        if (!result.Succeeded)
        {
            throw new JsonMergePatchException("Invalid JSON patch document.");
        }
    }

    public static void ApplyTo<T>(ref T obj, string patchString, JsonMergeOptions? mergeOptions = null)
    {
        var result = SafeApplyTo<T>(ref obj, patchString, mergeOptions);
        if (!result.Succeeded)
        {
            throw new JsonMergePatchException("Invalid JSON patch document.");
        }
    }

    public static JsonMergePatchResult SafeApplyTo<T>(ref T obj, string patchString,
        JsonMergeOptions? mergeOptions = null)
    {
        var patchBytes = Encoding.UTF8.GetBytes(patchString);

        return SafeApplyTo(ref obj, patchBytes, mergeOptions);
    }

    public static JsonMergePatchResult SafeApplyTo<T>(ref T obj, byte[] patchBytes,
        JsonMergeOptions? mergeOptions = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(patchBytes);

        mergeOptions ??= JsonMergeOptions.Default;

        var jsonOptions = mergeOptions.JsonSerializerOptions ?? JsonSerializerOptions.Default;

        var errors = new ErrorDictionary();
        var ops = new List<JsonMergePatchOperation>();

        var reader = new Utf8JsonReader(patchBytes);
        reader.Read();

        var typeInfo = jsonOptions.GetTypeInfo(obj.GetType());
        if (typeInfo.Kind is JsonTypeInfoKind.Object)
        {
            SafeApplyToObject(ref reader, ref obj, ref errors, ref ops, typeInfo, [], mergeOptions, jsonOptions);
        }
        else if (typeInfo.Kind is JsonTypeInfoKind.Dictionary)
        {
            SafeApplyToDictionary(ref reader, ref obj, ref errors, ref ops, typeInfo, [], mergeOptions, jsonOptions);
        }
        else if (typeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.None)
        {
            var converter = jsonOptions.GetConverter(typeof(T));

            try
            {
                var value = ReadValueWithConverter(ref reader, converter, typeof(T), jsonOptions);

                obj = (T)value!;
            }
            catch (Exception e)
            {
                errors.Add("~", $"Invalid value for this property. {e.Message}");
            }
        }
        else
        {
            throw new InvalidOperationException($"JsonTypeInfoKind {typeInfo.Kind} is not allowed.");
        }

        if (errors.Count > 0)
        {
            return JsonMergePatchResult.Fail(errors.ToArrayDictionary());
        }

        foreach (var op in ops)
        {
            op.Apply();
        }

        return JsonMergePatchResult.Success;
    }

    private static void SafeApplyToDictionary<TDict>(ref Utf8JsonReader reader, ref TDict dictionary,
        ref ErrorDictionary errors, ref List<JsonMergePatchOperation> ops, JsonTypeInfo typeInfo,
        string[] path, JsonMergeOptions mergeOptions, JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        reader.Read();
        
        if (typeInfo.KeyType is null)
        {
            throw new InvalidOperationException("TypeInfo.KeyType is null on a dictionary.");
        }

        if (typeInfo.ElementType is null)
        {
            throw new InvalidOperationException("TypeInfo.ElementType is null on a dictionary.");
        }

        var keyConverter = jsonOptions.GetConverter(typeInfo.KeyType);
        var elementConverter = jsonOptions.GetConverter(typeInfo.ElementType);
        
        while (reader.TokenType is not JsonTokenType.EndObject)
        {
            object? key = null;

            try
            {
                key = ReadValueAsPropertyNameWithConverter(ref reader, keyConverter, typeInfo.KeyType, jsonOptions);
            }
            catch (Exception e)
            {
                errors.Add(GetPropertyPath(path),
                    $"Invalid key value in this dictionary. {e.Message}");
            }

            if (key is null)
            {
                throw new InvalidOperationException("Dictionary key cannot be null.");
            }

            reader.Read();
            
            var elementTypeInfo = jsonOptions.GetTypeInfo(typeInfo.ElementType);
            if (elementTypeInfo.Kind is JsonTypeInfoKind.Object)
            {
                var existing = ((IDictionary)dictionary).Contains(key) ? ((IDictionary)dictionary)[key] : null;

                if (existing is null)
                {
                    var newValue = elementTypeInfo.CreateObject?.Invoke();

                    if (newValue is null)
                    {
                        errors.Add(GetPropertyPath(path, key.ToString()),
                            $"Property {GetPropertyPath(path, key.ToString())} is null and cannot be created.");

                        reader.Skip();
                        reader.Read();

                        continue;
                    }

                    existing = newValue;
                }
                
                SafeApplyToObject(ref reader, ref existing, ref errors, ref ops, elementTypeInfo, [..path, key.ToString()], mergeOptions, jsonOptions);
                
                ops.Add(new JsonMergePatchOperation(tgt => ((IDictionary)tgt)[key] = existing, dictionary));

            }
            else if (elementTypeInfo.Kind is JsonTypeInfoKind.Dictionary)
            {
                var existing = ((IDictionary)dictionary).Contains(key) ? ((IDictionary)dictionary)[key] : null;

                if (existing is null)
                {
                    var newValue = elementTypeInfo.CreateObject?.Invoke();
                    if (newValue is null)
                    {
                        errors.Add(GetPropertyPath(path, key.ToString()),
                            $"Property {GetPropertyPath(path, key.ToString())} is null and cannot be created.");

                        reader.Skip();
                        reader.Read();

                        continue;
                    }

                    existing = newValue;
                }
                
                SafeApplyToDictionary(ref reader, ref existing, ref errors, ref ops, elementTypeInfo, [..path, key.ToString()], mergeOptions, jsonOptions);
                
                ops.Add(new JsonMergePatchOperation(tgt => ((IDictionary)tgt)[key] = existing, dictionary));
            }
            else if (elementTypeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.None)
            {
                try
                {
                    var value = ReadValueWithConverter(ref reader, elementConverter, typeInfo.ElementType, jsonOptions);

                    ops.Add(new JsonMergePatchOperation(tgt => ((IDictionary)tgt)[key] = value, dictionary));
                }
                catch (Exception e)
                {
                    errors.Add("~", $"Invalid value for this property. {e.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException($"JsonTypeInfoKind {typeInfo.Kind} is not allowed.");
            }
            
            reader.Read();
        }
    }

    private static void SafeApplyToObject<TObj>(ref Utf8JsonReader reader, ref TObj obj,
        ref ErrorDictionary errors, ref List<JsonMergePatchOperation> ops, JsonTypeInfo typeInfo,
        string[] path, JsonMergeOptions mergeOptions, JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(obj);

        reader.Read();
        
        while (reader.TokenType is not JsonTokenType.EndObject)
        {
            if (reader.TokenType is not JsonTokenType.PropertyName)
            {
                throw new JsonMergePatchException("Invalid JSON. Expected property name.");
            }

            var propertyName = reader.GetString();
            if (propertyName is null)
            {
                throw new JsonMergePatchException($"Invalid JSON. Property name is null.");
            }
            
            var jsonProperty = typeInfo.Properties.FirstOrDefault(x => x.Name == propertyName);
            if (jsonProperty is null)
            {
                errors.Add("~", $"Property {GetPropertyPath(path, propertyName)} not found.");

                reader.Skip();
                reader.Read();

                continue;
            }

            if (jsonProperty.Set is null)
            {
                errors.Add(GetPropertyPath(path, propertyName),
                    $"Property {GetPropertyPath(path, propertyName)} has no setter.");

                reader.Skip();
                reader.Read();

                continue;
            }
            
            var securityPolicy = IsPropertyPatchable(jsonProperty, mergeOptions);
            if (securityPolicy != JsonMergeSecurityPolicy.AllowPatching)
            {
                if (securityPolicy == JsonMergeSecurityPolicy.BlockPatching)
                {
                    errors.Add(GetPropertyPath(path, propertyName),
                        $"Patching{GetPropertyPath(path, propertyName)} is prohibited.");
                }

                reader.Skip();
                reader.Read();

                continue;
            }
            
            reader.Read();

            var propertyTypeInfo = GetTypeInfo(jsonProperty);
            if (propertyTypeInfo.Kind is JsonTypeInfoKind.Object)
            {
                var currentObjectValue = jsonProperty.Get?.Invoke(obj);
                if (currentObjectValue is null)
                {
                    var newObjectValue = propertyTypeInfo.CreateObject?.Invoke();
                    if (newObjectValue is null)
                    {
                        errors.Add(GetPropertyPath(path, propertyName),
                            $"Property {GetPropertyPath(path, propertyName)} is null and cannot be created.");

                        reader.Skip();
                        reader.Read();

                        continue;
                    }
                    
                    ops.Add(new JsonMergePatchOperation(tgt => jsonProperty.Set(tgt, newObjectValue), obj));

                    currentObjectValue = newObjectValue;
                }
                
                SafeApplyToObject(ref reader, ref currentObjectValue, ref errors, ref ops, propertyTypeInfo, [..path, propertyName], mergeOptions, jsonOptions);
            }
            else if (propertyTypeInfo.Kind is JsonTypeInfoKind.Dictionary)
            {
                var currentDictionaryValue = jsonProperty.Get?.Invoke(obj);
                if (currentDictionaryValue is null)
                {
                    var newDictionaryValue = propertyTypeInfo.CreateObject?.Invoke();
                    if (newDictionaryValue is null)
                    {
                        errors.Add(GetPropertyPath(path, propertyName),
                            $"Property {GetPropertyPath(path, propertyName)} is null and cannot be created.");

                        reader.Skip();
                        reader.Read();

                        continue;
                    }
                    
                    ops.Add(new JsonMergePatchOperation(tgt => jsonProperty.Set(tgt, newDictionaryValue), obj));
                    
                    currentDictionaryValue = newDictionaryValue;
                }
                
                SafeApplyToDictionary(ref reader, ref currentDictionaryValue, ref errors, ref ops, propertyTypeInfo, [..path, propertyName], mergeOptions, jsonOptions);
            }
            else if (propertyTypeInfo.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.None)
            {
                var converter = jsonProperty.CustomConverter ?? jsonOptions.GetConverter(jsonProperty.PropertyType);

                try
                {
                    var value = ReadValueWithConverter(ref reader, converter, jsonProperty.PropertyType, jsonOptions);

                    ops.Add(new JsonMergePatchOperation(tgt => jsonProperty.Set(tgt, value), obj));
                }
                catch (Exception e)
                {
                    errors.Add("~", $"Invalid value for this property. {e.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException($"JsonTypeInfoKind {typeInfo.Kind} is not allowed.");
            }

            reader.Read();
        }
    }

    private static JsonMergeSecurityPolicy IsPropertyPatchable(JsonPropertyInfo propertyInfo,
        JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return mergeOptions.SecurityPolicy;
        }

        var attributes =
            propertyInfo.AttributeProvider.GetCustomAttributes(typeof(JsonMergePropertySecurityAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergePropertySecurityAttribute attribute)
        {
            return attribute.Policy;
        }

        return mergeOptions.SecurityPolicy;
    }
    
    private static string GetPropertyPath(string[] path, string propertyName)
    {
        return string.Join('.', [..path, propertyName]);
    }

    private static string GetPropertyPath(string[] path)
    {
        return string.Join('.', path);
    }

    private static JsonTypeInfo GetTypeInfo(JsonPropertyInfo property)
    {
        var p = typeof(JsonPropertyInfo).GetProperty("JsonTypeInfo", BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        return p.GetValue(property, null) as JsonTypeInfo;
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
    
    private static object? ReadValueAsPropertyNameWithConverter(ref Utf8JsonReader reader, JsonConverter converter, Type propertyType,
        JsonSerializerOptions jsonOptions)
    {
        var readDelegate = CreateGenericReadAsPropertyNameDelegate(propertyType);
        
        return readDelegate(converter, ref reader, propertyType, jsonOptions);
    }
    
    private delegate object? ReadAsPropertyNameDelegate(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o);

    private static ReadAsPropertyNameDelegate CreateGenericReadAsPropertyNameDelegate(Type valueType)
    {
        var m = typeof(JsonMergePatcher).GetMethod(nameof(ReadAsPropertyName), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);

        return m.CreateDelegate<ReadAsPropertyNameDelegate>();
    }

    private static object? ReadAsPropertyName<TValue>(JsonConverter c, ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => ((JsonConverter<TValue>)c).ReadAsPropertyName(ref r, t, o);
}