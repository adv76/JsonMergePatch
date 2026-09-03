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
/// Extension methods for adding OpenAPI metadata to ASP.NET Core endpoints.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds generic Accepts metadata to the route builder for JSON merge patch.
    /// </summary>
    /// <remarks>
    /// Sets the allowed content types to "application/merge-patch+json" and
    /// "application/json".
    /// </remarks>
    /// <param name="builder">The route builder to add the metadata to.</param>
    /// <returns>The route builder</returns>
    public static RouteHandlerBuilder AcceptsJsonMergePatch(this RouteHandlerBuilder builder)
        => builder.Accepts<object>("application/merge-patch+json", "application/json");

    /// <summary>
    /// Adds typed Accepts metadata to the route builder for JSON merge patch.
    /// </summary>
    /// <remarks>
    /// Builds a typed JsonMergePatch document using reflection for the type T. This
    /// gives the endpoint strongly typed patch document metdata, which will show
    /// in the OpenAPI spec and many OpenAPI viewer tools. Sets the allowed content
    /// types to "application/merge-patch+json" and "application/json".
    /// </remarks>
    /// <param name="builder">The route builder to add the metadata to.</param>
    /// <returns>The route builder</returns>
    public static RouteHandlerBuilder AcceptsTypedJsonMergePatch<T>(this RouteHandlerBuilder builder)
    {
        builder.Add(convention =>
        {
            
            var jsonOptions =  convention.ApplicationServices.GetService<IOptions<JsonOptions>>();
            var mergeOptions = convention.ApplicationServices.GetService<IOptions<JsonMergeOptions>>();
            
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
                
                convention.Metadata.Add(new AcceptsMetadata(["application/merge-patch+json", "application/json"], t, false));
            }
            else
            {
                convention.Metadata.Add(new AcceptsMetadata(["application/merge-patch+json", "application/json"], typeof(T), false));
            }
        });
        
        return builder;
    }
}