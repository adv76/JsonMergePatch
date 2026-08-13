using System.Reflection;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adv76.AspNetCore.JsonMergePatch;

public class JsonMergePatchDocument<T> : IBindableFromHttpContext<JsonMergePatchDocument<T>>
{
    private readonly JsonMergeOptions? _mergeOptions;
    private readonly string _jsonBodyString;
    
    private JsonMergePatchDocument(string jsonBodyString, JsonMergeOptions? mergeOptions = null)
    {
        _jsonBodyString = jsonBodyString;
        _mergeOptions = mergeOptions;
    }
    
    public void ApplyTo(ref T obj)
    {
        JsonMergePatcher.ApplyTo(ref obj, _jsonBodyString, _mergeOptions);
    }
    
    public JsonMergePatchResult SafeApplyTo(ref T obj)
    {
        return JsonMergePatcher.SafeApplyTo(ref obj, _jsonBodyString, _mergeOptions);
    }
    
    public static async ValueTask<JsonMergePatchDocument<T>?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>();
        var mergeOptions = context.RequestServices.GetService<IOptions<JsonMergeOptions>>();

        using var sr = new StreamReader(context.Request.Body);
        var bodyString = await sr.ReadToEndAsync();

        var merge = mergeOptions?.Value ?? JsonMergeOptions.Default;

        if (merge.JsonSerializerOptions is null && jsonOptions is not null)
        {
            merge.JsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        }
        
        return new JsonMergePatchDocument<T>(bodyString, merge);
    }
}