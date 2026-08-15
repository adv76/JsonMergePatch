namespace Adv76.JsonMergePatch;

/// <summary>
/// Security policies for merge patching.
/// </summary>
public enum JsonMergeSecurityPolicy
{
    /// <summary>
    /// Silently skip patching a property.
    /// </summary>
    SkipSilently = 1,
    
    /// <summary>
    /// Cause the patch to fail if a property is patched.
    /// </summary>
    BlockPatching = 2,
    
    /// <summary>
    /// Allow patching a property.
    /// </summary>
    AllowPatching = 3
}