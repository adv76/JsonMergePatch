using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Adv76.JsonMergePatch;

public static partial class JsonMergePatcher
{
    public static JsonMergePatchResult SafeApplyTo<T>(ref T obj, string patchString, JsonMergeOptions? mergeOptions = null)
    {
        var patchBytes = Encoding.UTF8.GetBytes(patchString);
        
        return SafeApplyTo(ref obj, patchBytes, mergeOptions);
    }
    
    public static JsonMergePatchResult SafeApplyTo<T>(ref T obj, byte[] patchBytes, JsonMergeOptions? mergeOptions = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(patchBytes);

        mergeOptions ??= JsonMergeOptions.Default;
        
        var jsonOptions = mergeOptions.JsonSerializerOptions ?? JsonSerializerOptions.Default;
        
        var errors = new Dictionary<string, string>();
        var ops = new List<JsonMergePatchOperation>();
        
        var reader = new Utf8JsonReader(patchBytes);
        
        reader.Read();
        
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var typeInfo = jsonOptions.GetTypeInfo(obj.GetType());
            
            SafeApplyToObject(ref reader, ref obj, ref errors, ref ops, typeInfo, [], mergeOptions, jsonOptions);
        }
        else
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
        
        if (errors.Count > 0)
        {
            return JsonMergePatchResult.Fail(errors);
        }

        foreach (var op in ops)
        {
            op.Apply();
        }

        return JsonMergePatchResult.Success;
    }

    private static void SafeApplyToObject<TInner>(ref Utf8JsonReader reader, ref TInner obj, ref Dictionary<string, string> errors, ref List<JsonMergePatchOperation> ops, JsonTypeInfo typeInfo, string[] path, JsonMergeOptions mergeOptions, JsonSerializerOptions jsonOptions)
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

            if (typeInfo.Kind == JsonTypeInfoKind.Object)
            {

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

                var securityPolicy = IsPropertyPatchable2(jsonProperty, mergeOptions);
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

                if (reader.TokenType == JsonTokenType.StartObject && !UseCustomConverterForObject(jsonProperty))
                {
                    var propertyObjectTypeInfo = jsonOptions.GetTypeInfo(jsonProperty.PropertyType);

                    var existing = jsonProperty.Get?.Invoke(obj);

                    if (existing is null)
                    {
                        var newValue = propertyObjectTypeInfo.CreateObject?.Invoke();

                        if (newValue is null)
                        {
                            errors.Add(GetPropertyPath(path, propertyName),
                                $"Property {GetPropertyPath(path, propertyName)} is null and cannot be created.");

                            reader.Skip();
                            reader.Read();

                            continue;
                        }

                        existing = newValue;
                    }

                    SafeApplyToObject(ref reader, ref existing, ref errors, ref ops, propertyObjectTypeInfo,
                        [..path, propertyName], mergeOptions, jsonOptions);

                    ops.Add(new JsonMergePatchOperation(tgt => jsonProperty.Set(tgt, existing), obj));
                }
                else
                {
                    var converter = (jsonProperty.CustomConverter ??
                                     jsonOptions.GetConverter(jsonProperty.PropertyType));

                    try
                    {
                        var value = ReadValueWithConverter(ref reader, converter, jsonProperty.PropertyType,
                            jsonOptions);

                        if (jsonProperty.IsRequired && value is null)
                        {
                            errors.Add(GetPropertyPath(path, propertyName),
                                $"Property {GetPropertyPath(path, propertyName)} is required and cannot be set to 'null'.");
                        }

                        ops.Add(new JsonMergePatchOperation(tgt => jsonProperty.Set(tgt, value), obj));
                    }
                    catch (Exception e)
                    {
                        errors.Add(GetPropertyPath(path, propertyName),
                            $"Invalid value for this property. {e.Message}");
                    }
                }

                reader.Read();
            } 
            else if (typeInfo.Kind == JsonTypeInfoKind.Dictionary)
            {
                // TODO double check
                var converter = jsonOptions.GetConverter(typeInfo.ElementType!);

                try
                {
                    var value = ReadValueWithConverter(ref reader, converter, typeInfo.ElementType!,
                        jsonOptions);
                    
                    ops.Add(new JsonMergePatchOperation(tgt => ((IDictionary)tgt)[propertyName] = value, obj));
                }
                catch (Exception e)
                {
                    errors.Add(GetPropertyPath(path, propertyName),
                        $"Invalid value for this property. {e.Message}");
                }
                
                reader.Read();
            }
            else
            {
                throw new NotImplementedException($"Type info kind {typeInfo.Kind} not implemented.");
            }
        }
    }
    
    private static JsonMergeSecurityPolicy IsPropertyPatchable2(JsonPropertyInfo propertyInfo, JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return mergeOptions.SecurityPolicy;
        }
        
        var attributes = propertyInfo.AttributeProvider.GetCustomAttributes(typeof(JsonMergePropertySecurityAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergePropertySecurityAttribute attribute)
        {
            return attribute.Policy;
        }

        return mergeOptions.SecurityPolicy;
    }
}