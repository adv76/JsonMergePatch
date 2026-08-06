using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch;

public static class PatchExtensions
{
    /// <summary>
    /// Applies a merge patch to an object
    /// </summary>
    /// <remarks>
    /// The patch follows a three-step process:
    /// 1. Serialize the object to a JsonNode
    /// 2. Apply the patch to the json object
    /// 3. Deserialize the JsonNode back to the original object type
    /// If the result of the patch is not serializable, the
    /// method throws a <see cref="JsonMergePatchException"/>.
    /// </remarks>
    /// <param name="obj">The object to patch</param>
    /// <param name="patch">The merge patch</param>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>The patched object</returns>
    public static T? MergePatch<T>(this T? obj, JsonNode? patch)
    {
        var jsonT = JsonSerializer.SerializeToNode(obj);

        var result = JsonMergePatcher.Merge(jsonT, patch);

        try
        {
            return result.Deserialize<T>();
        }
        catch (Exception e)
        {
            throw new JsonMergePatchException("An error occurred while applying the merge patch.", e);
        }
    }

    public static void MergePatch<T>(ref T? obj, JsonDocument? patch)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(patch);
        
        
    }
}