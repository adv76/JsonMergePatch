namespace Adv76.JsonMergePatch;

[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonMergeSecurityAttribute : Attribute
{
    public JsonMergeSecurityType SecurityType { get; init; }
}