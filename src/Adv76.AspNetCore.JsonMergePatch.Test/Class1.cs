using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Adv76.AspNetCore.JsonMergePatch.Test;

public class Class1
{
    public int Int1 { get; set; }
    public int? NullableInt1 { get; set; }
    public string String1 { get; set; } = string.Empty;
    public string? NullableString1  { get; set; }
    public required string RequiredString1 { get; set; }
    [JsonRequired]
    public string? RequiredString2 { get; set; }
}