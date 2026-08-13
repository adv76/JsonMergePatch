namespace Adv76.JsonMergePatch;

public sealed class JsonMergePatchResult
{
    public bool Succeeded { get; private init; }

    public Dictionary<string, string> Errors { get; private init; } = null!;

    private JsonMergePatchResult(bool succeeded, Dictionary<string, string>? errors = null)
    {
        Succeeded = succeeded;

        if (succeeded) return;
        
        ArgumentNullException.ThrowIfNull(errors);
            
        Errors = errors;
    }
    
    public static JsonMergePatchResult Success { get; } = new(true);

    public static JsonMergePatchResult Fail(string property, string error)
        => new(false, new Dictionary<string, string>() { { property, error } });
    
    public static JsonMergePatchResult Fail(Dictionary<string, string> errors)
        => new(false, errors);
}