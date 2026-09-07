using Microsoft.AspNetCore.OpenApi;

namespace Adv76.AspNetCore.JsonMergePatch.OpenApi;

/// <summary>
/// Extension methods for registering JsonMergePatch OpenAPI support.
/// </summary>
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// Adds the JsonMergePatch schema transformer, which populates the schema of
    /// <see cref="JsonMergePatchDocument{T}"/> with the merge-patch shape of
    /// <c>T</c> so the OpenAPI document shows all properties as optional and
    /// nullable, with blocked properties hidden.
    /// </summary>
    /// <param name="options">The OpenAPI options to add the transformer to.</param>
    /// <returns>The options, for chaining.</returns>
    public static OpenApiOptions AddJsonMergePatch(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddSchemaTransformer<JsonMergePatchSchemaTransformer>();

        return options;
    }
}
