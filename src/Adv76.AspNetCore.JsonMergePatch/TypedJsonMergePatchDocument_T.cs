using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adv76.AspNetCore.JsonMergePatch;

/// <summary>
/// A strongly typed JsonMergePatch document
/// </summary>
/// <remarks>
/// The JSON merge patch is automatically read from the HTTP request body.
///
/// This class will automatically add a Reflection-based strongly typed patch
/// document to the Accepts metadata. It is the equivalent to calling
/// AcceptsTypedJsonMergePatch&gt;T&lt; in <see cref="OpenApiExtensions"/>.
/// </remarks>
/// <typeparam name="T">The type that the patch is for.</typeparam>
public class TypedJsonMergePatchDocument<T> : IBindableFromHttpContext<TypedJsonMergePatchDocument<T>>,
    IEndpointParameterMetadataProvider
{
    private readonly JsonMergeOptions? _mergeOptions;
    private readonly string _jsonBodyString;

    private TypedJsonMergePatchDocument(string jsonBodyString, JsonMergeOptions? mergeOptions = null)
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

    /// <<inheritdoc/>
    public static async ValueTask<TypedJsonMergePatchDocument<T>?> BindAsync(HttpContext context,
        ParameterInfo parameter)
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

        return new TypedJsonMergePatchDocument<T>(bodyString, merge);
    }

    /// <<inheritdoc/>
    public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(builder);

        var jsonOptions = builder.ApplicationServices.GetService<IOptions<JsonOptions>>();
        var mergeOptions = builder.ApplicationServices.GetService<IOptions<JsonMergeOptions>>();

        var merge = mergeOptions?.Value ?? JsonMergeOptions.Default;

        if (merge.JsonSerializerOptions is null && jsonOptions is not null)
        {
            merge.JsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        }

        var jsonSerializerOptions = merge.JsonSerializerOptions ?? JsonSerializerOptions.Default;

        var jsonTypeInfo = jsonSerializerOptions.GetTypeInfo(typeof(T));

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            var t = jsonTypeInfo.BuildPatchType(merge, jsonSerializerOptions);

            builder.Metadata.Add(new AcceptsMetadata(["application/merge-patch+json", "application/json"], t, false));
        }
        else
        {
            builder.Metadata.Add(new AcceptsMetadata(["application/merge-patch+json", "application/json"], typeof(T),
                false));
        }
    }
}