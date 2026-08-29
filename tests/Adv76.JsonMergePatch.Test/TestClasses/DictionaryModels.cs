namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class PrimitiveDictionaryModel
{
    public Dictionary<string, int> Dictionary1 { get; set; } = [];
    public Dictionary<int, int> Dictionary2 { get; set; } = [];
}

internal class ObjectDictionaryModel
{
    public Dictionary<string, SimpleModel> Dictionary1 { get; set; } = [];
}

internal class NestedDictionaryModel
{
    public Dictionary<string, Dictionary<string, int>> NestedDict { get; set; } = [];
}

internal class ComplexDictionaryModel
{
    public Dictionary<string, Dictionary<string, SimpleModel>> DeepObjectDict { get; set; } = [];
}

internal class DictionaryWithObjectAndPrimitiveModel
{
    public Dictionary<string, SimpleModel> ObjectDict { get; set; } = [];
    public Dictionary<string, int> PrimitiveDict { get; set; } = [];
}
