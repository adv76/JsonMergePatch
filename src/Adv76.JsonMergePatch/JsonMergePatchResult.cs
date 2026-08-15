namespace Adv76.JsonMergePatch;

/// <summary>
/// Result object for JSON Merge Patches
/// </summary>
public sealed class JsonMergePatchResult
{
    /// <summary>
    /// Whether the patch succeeded.
    /// </summary>
    public bool Succeeded { get; private init; }

    /// <summary>
    /// The errors that were encountered while attempting the patch.
    /// </summary>
    /// <remarks>
    /// This is only set if <see cref="Succeeded"/> is false. If succeeded is true, it will be null.
    /// </remarks>
    public Dictionary<string, string> Errors { get; private init; } = null!;

    private JsonMergePatchResult(bool succeeded, Dictionary<string, string>? errors = null)
    {
        Succeeded = succeeded;

        if (succeeded) return;
        
        ArgumentNullException.ThrowIfNull(errors);
            
        Errors = errors;
    }
    
    /// <summary>
    /// Creates a new successful patch result.
    /// </summary>
    public static JsonMergePatchResult Success { get; } = new(true);

    /// <summary>
    /// Creates a patch failure result.
    /// </summary>
    /// <param name="errors">The errors that occurred.</param>
    /// <returns>An error patch result containing the provided errors.</returns>
    public static JsonMergePatchResult Fail(Dictionary<string, string> errors)
        => new(false, errors);
}