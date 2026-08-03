using System.Reflection;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adv76.AspNetCore.JsonMergePatch;

public class JsonMergePatchDocument<T> : IBindableFromHttpContext<JsonMergePatchDocument<T>>
{
    private readonly JsonMergePatch<T> _patch;

    internal JsonMergePatchDocument(JsonMergePatch<T> patch)
    {
        _patch = patch;
    }
    
    public void ApplyTo(ref T obj)
    {
        _patch.ApplyTo(ref obj);
    }
    
    public static async ValueTask<JsonMergePatchDocument<T>?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>();

        using var sr = new StreamReader(context.Request.Body);
        var bodyString = await sr.ReadToEndAsync();

        return new JsonMergePatchDocument<T>(new JsonMergePatch<T>(bodyString, jsonOptions?.Value.SerializerOptions));
    }
}