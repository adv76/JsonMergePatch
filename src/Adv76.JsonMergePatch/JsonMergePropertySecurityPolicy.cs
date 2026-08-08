namespace Adv76.JsonMergePatch;

public enum JsonMergePropertySecurityPolicy
{
    /// <summary>
    /// Silently skip patching the property
    /// </summary>
    SkipSilently = 1,
    
    /// <summary>
    /// Cause the patch to fail if the property is patched
    /// </summary>
    BlockPatching = 2,
    
    /// <summary>
    /// Allow patching this property
    /// </summary>
    AllowPatching = 3
}