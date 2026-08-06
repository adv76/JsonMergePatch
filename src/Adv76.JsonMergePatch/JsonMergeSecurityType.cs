namespace Adv76.JsonMergePatch;

public enum JsonMergeSecurityType
{
    /// <summary>
    /// Cause the patch to fail if the property is patched
    /// </summary>
    BlockPatch = 0,
    
    /// <summary>
    /// Silently skip patching the property
    /// </summary>
    SkipSilently = 1
}