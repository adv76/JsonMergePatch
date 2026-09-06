using Microsoft.AspNetCore.OpenApi;

namespace Adv76.AspNetCore.JsonMergePatch.OpenApi;

/// <summary>
/// Extension methods for registering JsonMergePatch OpenAPI support.
/// </summary>
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// Adds the JsonMergePatch operation transformer, which rewrites the request-body
    /// schema of endpoints whose body type is <see cref="JsonMergePatchDocument{T}"/>
    /// so the OpenAPI document shows the merge-patch shape (all properties optional
    /// and nullable, blocked properties hidden).
    /// </summary>
    /// <param name="options">The OpenAPI options to add the transformer to.</param>
    /// <returns>The options, for chaining.</returns>
    public static OpenApiOptions AddJsonMergePatch(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddOperationTransformer<JsonMergePatchOperationTransformer>();

        return options;
    }
}
