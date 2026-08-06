using System.Reflection;
using System.Text.Json;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adv76.AspNetCore.JsonMergePatch;

public class JsonMergePatchDocument<T> : IBindableFromHttpContext<JsonMergePatchDocument<T>>
{
    private readonly JsonSerializerOptions? _jsonOptions;
    private readonly string _jsonBodyString;
    
    private JsonMergePatchDocument(string jsonBodyString, JsonSerializerOptions? jsonOptions = null)
    {
        _jsonBodyString = jsonBodyString;
        _jsonOptions = jsonOptions;
    }
    
    public void ApplyTo(ref T obj)
    {
        JsonMergePatcher.ApplyTo(ref obj, _jsonBodyString, _jsonOptions);
    }
    
    public static async ValueTask<JsonMergePatchDocument<T>?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>();

        using var sr = new StreamReader(context.Request.Body);
        var bodyString = await sr.ReadToEndAsync();

        return new JsonMergePatchDocument<T>(bodyString, jsonOptions?.Value.SerializerOptions);
    }
}