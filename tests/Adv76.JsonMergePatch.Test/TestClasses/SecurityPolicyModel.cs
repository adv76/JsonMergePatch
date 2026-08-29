namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class SecurityPolicyModel
{
    [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.AllowPatching)]
    public string? AllowedString { get; set; }

    [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.BlockPatching)]
    public string? BlockedString { get; set; }

    [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.SkipSilently)]
    public string? IgnoredString { get; set; }

    public string? String { get; set; }
}
