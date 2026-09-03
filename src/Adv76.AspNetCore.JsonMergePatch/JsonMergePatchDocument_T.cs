using System.Reflection;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adv76.AspNetCore.JsonMergePatch;

/// <summary>
/// A JsonMergePatch document
/// </summary>
/// <remarks>
/// The JSON merge patch is automatically read from the HTTP request body.
/// </remarks>
/// <typeparam name="T">The type that the patch is for.</typeparam>
public class JsonMergePatchDocument<T> : IBindableFromHttpContext<JsonMergePatchDocument<T>>
{
    private readonly JsonMergeOptions? _mergeOptions;
    private readonly string _jsonBodyString;
    
    private JsonMergePatchDocument(string jsonBodyString, JsonMergeOptions? mergeOptions = null)
    {
        _jsonBodyString = jsonBodyString;
        _mergeOptions = mergeOptions;
    }
    
    /// <summary>
    /// Applies the patch to an object
    /// </summary>
    /// <remarks>
    /// This method applies the patch to the object using <see cref="JsonMergePatcher"/> ApplyTo.
    /// It will throw if the patch is invalid.
    /// </remarks>
    /// <param name="obj">The object to patch.</param>
    public void ApplyTo(ref T obj)
    {
        JsonMergePatcher.ApplyTo(ref obj, _jsonBodyString, _mergeOptions);
    }
    
    /// <summary>
    /// Applies the patch to an object
    /// </summary>
    /// /// <remarks>
    /// This method applies the patch to the object using <see cref="JsonMergePatcher"/> SafeApplyTo.
    /// It will not throw if the patch is invalid.
    /// </remarks>
    /// <param name="obj">The object to patch.</param>
    /// <returns>The result of the patch operation.</returns>
    public JsonMergePatchResult SafeApplyTo(ref T obj)
    {
        return JsonMergePatcher.SafeApplyTo(ref obj, _jsonBodyString, _mergeOptions);
    }
    
    /// <inheritdoc/>
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