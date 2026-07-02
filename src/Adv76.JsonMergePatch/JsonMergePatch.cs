using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch;

public static class JsonMergePatch
{
    public static JsonNode? Merge(JsonNode? obj, JsonNode? patch)
    {
        if (patch is null || patch.GetValueKind() is JsonValueKind.Null)
        {
            return JsonValue.Create((string?)null);
        }

        if (patch.GetValueKind() is not JsonValueKind.Object)
        {
            return patch.DeepClone();
        }
        
        if (obj?.GetValueKind() is not JsonValueKind.Object)
        {
            obj = new JsonObject();
        }
            
        return MergeObject(obj.AsObject(), patch.AsObject());
    }
    
    public static JsonObject MergeObject(JsonObject obj, JsonObject patch)
    {
        if (patch.GetValueKind() is not JsonValueKind.Object)
        {
            return obj;
        }
        
        foreach (var (key, value) in patch)
        {
            if (value is null || value.GetValueKind() is JsonValueKind.Null)
            {
                obj.Remove(key);
            }
            else
            {
                obj[key] = Merge(obj[key], value);
            }
        }

        return obj;
    }
}