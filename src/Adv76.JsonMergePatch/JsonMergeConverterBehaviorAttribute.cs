namespace Adv76.JsonMergePatch;

[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonMergeConverterBehaviorAttribute : Attribute
{
    public JsonMergeConverterBehavior Behavior { get; init; }
}