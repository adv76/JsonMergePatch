using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Adv76.AspNetCore.JsonMergePatch.OpenApi;

/// <summary>
/// Populates the schema of <see cref="JsonMergePatchDocument{T}"/> with the merge-patch
/// shape of <c>T</c> so the OpenAPI document shows the patchable properties.
/// </summary>
/// <remarks>
/// <para>
/// The transformer only acts when the generated schema is for
/// <see cref="JsonMergePatchDocument{T}"/>. All other schemas are left untouched.
/// </para>
/// <para>
/// Patch semantics applied to the schema:
/// <list type="bullet">
/// <item>All properties are optional (<c>required</c> is cleared).</item>
/// <item>Every kept property accepts <c>null</c> (flat <c>type</c> gains a
/// <c>Null</c> flag; composed/reference schemas are wrapped in <c>anyOf</c> with a
/// null branch).</item>
/// <item>Properties blocked by <see cref="JsonMergePropertySecurityAttribute"/> or the
/// <see cref="JsonMergeOptions.SecurityPolicy"/> default are hidden.</item>
/// <item>Nested POCOs recurse into their own <c>JsonMergePatch{T}Name</c> component schemas.</item>
/// </list>
/// </para>
/// <para>
/// Register once:
/// <code>builder.Services.AddOpenApi(options => options.AddJsonMergePatch());</code>
/// </para>
/// </remarks>
public sealed class JsonMergePatchSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc/>
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        var schemaType = context.JsonTypeInfo?.Type;
        if (schemaType is null
            || !schemaType.IsGenericType
            || schemaType.GetGenericTypeDefinition() != typeof(JsonMergePatchDocument<>))
        {
            return;
        }

        var targetType = schemaType.GetGenericArguments()[0];

        var jsonOpts = context.ApplicationServices.GetService<IOptions<JsonOptions>>();
        var mergeOpts = context.ApplicationServices.GetService<IOptions<JsonMergeOptions>>();

        var jsonMergeOptions = mergeOpts?.Value ?? JsonMergeOptions.Default;
        var jsonSerializerOptions = jsonOpts?.Value.SerializerOptions ?? jsonMergeOptions.JsonSerializerOptions ?? JsonSerializerOptions.Default;

        try
        {
            var typeInfo = jsonSerializerOptions.GetTypeInfo(targetType);
            if (typeInfo.Kind is not JsonTypeInfoKind.Object)
            {
                return;
            }

            var properties = await BuildPatchPropertiesAsync(
                typeInfo,
                context,
                jsonSerializerOptions,
                jsonMergeOptions,
                cancellationToken);

            schema.Type = JsonSchemaType.Object;
            schema.Description =
                $"JSON merge patch (RFC 7396) document for {typeInfo.Type.Name}. All properties are optional.";
            schema.Required = new HashSet<string>(StringComparer.Ordinal);
            schema.Properties = properties;
        }
        catch
        {
            // Leave the schema untouched when patch shaping fails.
        }
    }

    private static async Task<Dictionary<string, IOpenApiSchema>> BuildPatchPropertiesAsync(
        JsonTypeInfo typeInfo,
        OpenApiSchemaTransformerContext context,
        JsonSerializerOptions serializerOptions,
        JsonMergeOptions mergeOptions,
        CancellationToken cancellationToken)
    {
        var properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);

        foreach (var property in typeInfo.Properties)
        {
            try
            {
                if (!IsPropertyPatchable(property, mergeOptions))
                {
                    continue;
                }

                properties[property.Name] = await BuildPropertySchemaAsync(
                    property,
                    context,
                    serializerOptions,
                    cancellationToken);
            }
            catch
            {
                // Skip properties whose schema cannot be shaped.
            }
        }

        return properties;
    }

    private static async Task<IOpenApiSchema> BuildPropertySchemaAsync(
        JsonPropertyInfo property,
        OpenApiSchemaTransformerContext context,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken)
    {
        var typeInfo = serializerOptions.GetTypeInfo(property.PropertyType);

        if (IsPatchableObject(typeInfo))
        {
            return await GetPatchSchemaAsync(property.PropertyType, context, cancellationToken);
        }

        if (typeInfo.Kind is JsonTypeInfoKind.Dictionary && typeInfo.ElementType is not null)
        {
            var valueSchema = await BuildDictionaryValueSchemaAsync(
                typeInfo.ElementType,
                context,
                serializerOptions,
                cancellationToken);

            var baseSchema = await context
                .GetOrCreateSchemaAsync(property.PropertyType, null, cancellationToken);

            if (valueSchema is null)
            {
                return MakeNullable(baseSchema);
            }

            return WithPatchedAdditionalProperties(baseSchema, valueSchema);
        }

        var leafSchema = await context
            .GetOrCreateSchemaAsync(property.PropertyType, null, cancellationToken);

        return MakeNullable(leafSchema);
    }

    /// <summary>
    /// Builds the patch-shaped schema for a dictionary value type, or returns
    /// <c>null</c> when the values need no patch shaping (e.g. primitives).
    /// </summary>
    private static async Task<IOpenApiSchema?> BuildDictionaryValueSchemaAsync(
        Type elementType,
        OpenApiSchemaTransformerContext context,
        JsonSerializerOptions serializerOptions,
        CancellationToken cancellationToken)
    {
        var elementInfo = serializerOptions.GetTypeInfo(elementType);

        if (IsPatchableObject(elementInfo))
        {
            return await GetPatchSchemaAsync(elementType, context, cancellationToken);
        }

        if (elementInfo.Kind is JsonTypeInfoKind.Dictionary && elementInfo.ElementType is not null)
        {
            var nestedSchema = await BuildDictionaryValueSchemaAsync(
                elementInfo.ElementType,
                context,
                serializerOptions,
                cancellationToken);

            if (nestedSchema is null)
            {
                return null;
            }

            var baseSchema = await context
                .GetOrCreateSchemaAsync(elementType, null, cancellationToken);

            return WithPatchedAdditionalProperties(baseSchema, nestedSchema);
        }

        return null;
    }

    private static bool IsPatchableObject(JsonTypeInfo typeInfo)
        => typeInfo.Kind is JsonTypeInfoKind.Object && typeInfo.Type != typeof(object);

    private static async Task<IOpenApiSchema> GetPatchSchemaAsync(
        Type elementType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var wrapperType = typeof(JsonMergePatchDocument<>).MakeGenericType(elementType);
        var patchSchema = await context.GetOrCreateSchemaAsync(wrapperType, null, cancellationToken);

        return MakeNullable(patchSchema);
    }

    private static IOpenApiSchema WithPatchedAdditionalProperties(IOpenApiSchema baseSchema, IOpenApiSchema valueSchema)
    {
        var copy = (OpenApiSchema)baseSchema.CreateShallowCopy();
        copy.AdditionalProperties = valueSchema;

        return MakeNullable(copy);
    }

    private static IOpenApiSchema MakeNullable(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema concrete && concrete.Type.HasValue)
        {
            if (concrete.Type.Value.HasFlag(JsonSchemaType.Null))
            {
                return concrete;
            }

            var copy = (OpenApiSchema)concrete.CreateShallowCopy();
            copy.Type |= JsonSchemaType.Null;
            return copy;
        }

        return new OpenApiSchema
        {
            AnyOf =
            [
                schema,
                new OpenApiSchema { Type = JsonSchemaType.Null },
            ],
        };
    }

    private static bool IsPropertyPatchable(JsonPropertyInfo propertyInfo, JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return mergeOptions.SecurityPolicy == JsonMergeSecurityPolicy.AllowPatching;
        }

        var attributes =
            propertyInfo.AttributeProvider.GetCustomAttributes(typeof(JsonMergePropertySecurityAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergePropertySecurityAttribute attribute)
        {
            return attribute.Policy == JsonMergeSecurityPolicy.AllowPatching;
        }

        return mergeOptions.SecurityPolicy == JsonMergeSecurityPolicy.AllowPatching;
    }
}
