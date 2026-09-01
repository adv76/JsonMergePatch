using System.Runtime.Serialization;

namespace Adv76.JsonMergePatch;

/// <summary>
/// Exception class for when an error occurs
/// </summary>
/// <remarks>
/// <see cref="JsonMergePatcher"/> ApplyTo throws when the patch failed. The errors will
/// populated in the <see cref="Errors"/> dictionary.
/// </remarks>
public sealed class JsonMergePatchException : Exception
{
    /// <summary>
    /// The errors that caused the exception
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; }

    internal JsonMergePatchException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
}