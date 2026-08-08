namespace Adv76.JsonMergePatch;

public enum JsonMergeSecurityPolicy
{
    /// <summary>
    /// Block all patching unless it is explicitly allowed
    /// </summary>
    BlockByDefault = 0,
    
    /// <summary>
    /// Allow all patching unless it is explicitly forbidden
    /// </summary>
    AllowByDefault = 1,
}