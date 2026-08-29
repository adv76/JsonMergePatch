namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class NestedModel
{
    public SimpleModel? Inner { get; set; }
    public string? String1 { get; set; }
}

internal class DoubleNestedModel
{
    public NestedModel? Nested { get; set; }
    public string? Name { get; set; }
}
