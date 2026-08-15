namespace Adv76.JsonMergePatch;

/// <summary>
/// Sets a custom security policy on a property.
/// </summary>
/// <remarks>
/// This will override the default assigned in <see cref="JsonMergeOptions"/>. It allows
/// customization of the security policy on individual properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonMergePropertySecurityAttribute : Attribute
{
    /// <summary>
    /// The security policy for this property.
    /// </summary>
    public required JsonMergeSecurityPolicy Policy { get; init; }
}