using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Adv76.AspNetCore.JsonMergePatch;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Adv76.AspNetCore.JsonMergePatch.OpenApi;

/// <summary>
/// Populates the schema of <see cref="JsonMergePatchDocument{T}"/> with the merge-patch
/// shape of <c>T</c> so the OpenAPI document shows the patchable properties without
/// emitting runtime types.
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
/// <item>Nested POCOs recurse into their own <c>JsonMergePatch{T}Name</c> component schemas.
/// Collections/dictionaries keep the framework schema (RFC 7396 replaces them wholesale).</item>
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
        if (schemaType is null)
        {
            return;
        }

        if (!schemaType.IsGenericType || schemaType.GetGenericTypeDefinition() != typeof(JsonMergePatchDocument<>))
        {
            return;
        }
        
        var targetType = schemaType.GetGenericArguments()[0];
        
        var jsonOpts = context.ApplicationServices.GetService<IOptions<JsonOptions>>();
        var mergeOpts = context.ApplicationServices.GetService<IOptions<JsonMergeOptions>>();

        var jsonMergeOptions = mergeOpts?.Value ?? JsonMergeOptions.Default;

        if (jsonMergeOptions.JsonSerializerOptions is null && jsonOpts is not null)
        {
            jsonMergeOptions.JsonSerializerOptions = jsonOpts.Value.SerializerOptions;
        }

        var jsonSerializerOptions = jsonMergeOptions.JsonSerializerOptions ?? JsonSerializerOptions.Default;
        
        JsonTypeInfo typeInfo;
        try
        {
            typeInfo = jsonSerializerOptions.GetTypeInfo(targetType);
        }
        catch
        {
            return;
        }

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        try
        {
            var properties = await BuildPatchPropertiesAsync(
                typeInfo,
                context,
                jsonSerializerOptions,
                jsonMergeOptions,
                cancellationToken).ConfigureAwait(false);
            
            schema.Type = JsonSchemaType.Object;
            schema.Description =
                $"JSON merge patch (RFC 7396) document for {typeInfo.Type.Name}. All properties are optional.";
            schema.Required = new HashSet<string>();
            schema.Properties = properties;
        }
        catch
        {
            // Leave the generated schema untouched when patch shaping fails.
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
            if (!IsPropertyPatchable(property, mergeOptions))
            {
                continue;
            }

            IOpenApiSchema propertySchema;

            JsonTypeInfo? nested;
            try
            {
                nested = serializerOptions.GetTypeInfo(property.PropertyType);
            }
            catch
            {
                nested = null;
            }

            if (nested is not null && nested.Kind == JsonTypeInfoKind.Object && nested.Type != typeof(object))
            {
                OpenApiSchema baseSchema;

                var nestedType = typeof(JsonMergePatchDocument<>).MakeGenericType(property.PropertyType);
                
                try
                {
                    baseSchema = await context
                        .GetOrCreateSchemaAsync(nestedType, null, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch
                {
                    continue;
                }

                propertySchema = MakeNullable(baseSchema);
            }
            else
            {
                OpenApiSchema baseSchema;
                try
                {
                    baseSchema = await context
                        .GetOrCreateSchemaAsync(property.PropertyType, null, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch
                {
                    continue;
                }

                propertySchema = MakeNullable(baseSchema);
            }

            properties[property.Name] = propertySchema;
        }

        return properties;
    }

    private static IOpenApiSchema MakeNullable(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema concrete && concrete.Type.HasValue)
        {
            if ((concrete.Type.Value & JsonSchemaType.Null) != 0)
            {
                return concrete;
            }

            var copy = (OpenApiSchema)concrete.CreateShallowCopy();
            copy.Type |= JsonSchemaType.Null;
            return copy;
        }

        return new OpenApiSchema
        {
            AnyOf = new List<IOpenApiSchema>
            {
                schema,
                new OpenApiSchema { Type = JsonSchemaType.Null },
            },
        };
    }

    private static bool IsPropertyPatchable(JsonPropertyInfo propertyInfo, JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return false;
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
