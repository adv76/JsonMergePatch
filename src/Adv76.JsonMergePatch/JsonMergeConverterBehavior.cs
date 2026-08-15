namespace Adv76.JsonMergePatch;

/// <summary>
/// Converter behavior for parsing property values.
/// </summary>
public enum JsonMergeConverterBehavior
{
    /// <summary>
    /// Use the default parsing logic.
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// Always use a custom converter to deserialize the property.
    /// </summary>
    UseCustomConverter = 1
}