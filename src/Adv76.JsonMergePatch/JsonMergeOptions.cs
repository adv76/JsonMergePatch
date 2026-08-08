using System.Text.Json;

namespace Adv76.JsonMergePatch;

/// <summary>
/// Provides options to be used with <see cref="JsonMergePatcher"/>.
/// </summary>
public sealed class JsonMergeOptions
{
    /// <summary>
    /// The default security policy for updating properties
    /// </summary>
    /// <remarks>
    /// Any individual property configurations will supersede this. This policy only applies
    /// to properties not annotated with <see cref="JsonMergePropertySecurityAttribute"/>.
    /// 
    /// Defaults to <see cref="JsonMergeSecurityPolicy.AllowByDefault"/>.
    /// </remarks>
    public JsonMergeSecurityPolicy SecurityPolicy { get; set; } = JsonMergeSecurityPolicy.AllowByDefault;
    
    /// <summary>
    /// The serializer options to use for the deserialization and merging operations
    /// </summary>
    /// <remarks>
    /// The patcher defaults to <see cref="JsonSerializerOptions.Default"/> if not set.
    /// </remarks>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// The default options
    /// </summary>
    public static JsonMergeOptions Default => new();
    
    /// <summary>
    /// Strict options preset that requires manual opt-in to merging on
    /// a property-by-property basis.
    /// </summary>
    public static JsonMergeOptions Strict => new JsonMergeOptions()
    {
        SecurityPolicy = JsonMergeSecurityPolicy.BlockByDefault
    };
}