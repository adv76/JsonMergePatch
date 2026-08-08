using System.Runtime.Serialization;

namespace Adv76.JsonMergePatch;

public sealed class JsonMergePatchException : Exception
{
    private JsonMergePatchException()
    {
    }

    public JsonMergePatchException(string message) : base(message)
    {
    }

    public JsonMergePatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}