namespace Adv76.JsonMergePatch;

/// <summary>
/// Sets a custom converter behavior for a property
/// </summary>
/// <remarks>
/// The merge patcher uses custom converters by default to parse properties.
/// However, there can be issues if a custom converter outputs an object instead
/// of a value. According to the merge patch spec, non-objects are always replaced
/// by the value in the patch.
///
/// If a custom converter inputs and outputs and object for a value, the merge patcher
/// will attempt to patch the individual properties in the object instead of deserializing
/// the whole object. Adding this attribute with a behavior of
/// <see cref="JsonMergeConverterBehavior.UseCustomConverter"/> will tell the merge patcher
/// to read the whole object using the custom converter.
///
/// This attribute only needs to be added when using a custom converter if the custom converter
/// outputs an object instead of a value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonMergeConverterBehaviorAttribute : Attribute
{
    /// <summary>
    /// The converter behavior for a property.
    /// </summary>
    public JsonMergeConverterBehavior Behavior { get; init; }
}