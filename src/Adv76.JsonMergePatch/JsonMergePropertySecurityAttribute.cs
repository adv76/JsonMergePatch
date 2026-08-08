namespace Adv76.JsonMergePatch;

[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonMergePropertySecurityAttribute : Attribute
{
    public required JsonMergePropertySecurityPolicy Policy { get; init; }
}