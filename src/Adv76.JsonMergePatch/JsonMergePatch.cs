using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch;

public static class JsonMergePatch
{
    public static JsonObject Merge(JsonObject obj, JsonObject patch)
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
                    obj[key] = value.DeepClone();
                }
            }
        }

        return obj;
    }
}