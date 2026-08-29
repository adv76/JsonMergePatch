using System.Text.Json;

namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class RfcStringDictWrapper
{
    public Dictionary<string, string?> Data { get; set; } = [];
}

internal class RfcIntDictWrapper
{
    public Dictionary<string, int> Data { get; set; } = [];
}

internal class RfcNestedStringDictWrapper
{
    public Dictionary<string, Dictionary<string, string?>> Data { get; set; } = [];
}

internal class RfcArrayModel
{
    public string? Title { get; set; }
    public string[] Tags { get; set; } = [];
    public Dictionary<string, string?> Author { get; set; } = [];
}

internal class RfcGenericJsonModel
{
    public Dictionary<string, JsonElement> Dict { get; set; } = [];
}
