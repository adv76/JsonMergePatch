namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class ReadOnlyModel
{
    public string? ReadOnlyProp { get; } = "initial";
    public string? WritableProp { get; set; }
}

internal class ChildWithNoParameterlessCtor
{
    public ChildWithNoParameterlessCtor(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}

internal class ParentWithNoCtorChild
{
    public ChildWithNoParameterlessCtor? Child { get; set; }
    public string? Name { get; set; }
}

internal class WrapperModel
{
    public SimpleModel? Inner { get; set; }
    public Dictionary<string, SimpleModel> Map { get; set; } = [];
}

internal class MultiDictModel
{
    public Dictionary<string, Dictionary<string, Dictionary<string, int>>> TripleNested { get; set; } = [];
}
