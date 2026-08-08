using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Adv76.JsonMergePatch;

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
    [JsonMergePropertySecurity(Policy = JsonMergePropertySecurityPolicy.BlockPatching)]
    public string? ProhibitedString1 { get; set; }
    [JsonMergePropertySecurity(Policy = JsonMergePropertySecurityPolicy.SkipSilently)]
    public string? ReadonlyString1 { get; set; }
}