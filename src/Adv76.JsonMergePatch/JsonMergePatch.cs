using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch;

public static class JsonMergePatch
{
    public static JsonNode Merge(JsonNode obj, JsonNode patch)
    {
        if (patch is null || patch.GetValueKind() is JsonValueKind.Null)
        {
            return JsonValue.Create((string?)null);
        }
        else if (patch.GetValueKind() is JsonValueKind.Object)
        {
            if (obj is null || obj.GetValueKind() is not JsonValueKind.Object)
            {
                obj = new JsonObject();
            }
            
            return MergeObject(obj as JsonObject, patch as JsonObject);
        }
        else
        {
            return patch.DeepClone();
        }

        return obj;
    }
    
    public static JsonObject MergeObject(JsonObject obj, JsonObject patch)
    {
        //throw new NotImplementedException();

        if (patch.GetValueKind() is JsonValueKind.Object)
        {
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
        }

        return obj;
    }
}