using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Adv76.AspNetCore.JsonMergePatch.OpenApi;

/// <summary>
/// Rewrites the request-body schema of <c>PATCH</c> endpoints whose body type is
/// <see cref="JsonMergePatchDocument{T}"/> so the OpenAPI document shows a merge-patch
/// shape without emitting runtime types.
/// </summary>
/// <remarks>
/// <para>
/// The transformer only acts when a handler parameter (or <see cref="AcceptsMetadata"/>)
/// uses <see cref="JsonMergePatchDocument{T}"/>. All other endpoints are left untouched.
/// </para>
/// <para>
/// Patch semantics applied to the schema, mirroring the old reflection-emit behavior:
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
public sealed class JsonMergePatchOperationTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Document);

        var targetType = GetPatchTargetType(context);
        if (targetType is null)
        {
            return;
        }

        var jsonOptions = context.ApplicationServices.GetService<IOptions<JsonOptions>>();
        var mergeOptions = context.ApplicationServices.GetService<IOptions<JsonMergeOptions>>();
        
        var merge = mergeOptions?.Value ?? JsonMergeOptions.Default;

        if (merge.JsonSerializerOptions is null && jsonOptions is not null)
        {
            merge.JsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        }

        var jsonSerializerOptions = merge.JsonSerializerOptions ?? JsonSerializerOptions.Default;

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

        IOpenApiSchema patchRef;
        try
        {
            patchRef = await BuildPatchSchemaAsync(
                typeInfo.Type,
                typeInfo,
                context,
                jsonSerializerOptions,
                merge,
                new Dictionary<Type, string>(),
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return;
        }

        if (operation.RequestBody?.Content is null || operation.RequestBody.Content.Count == 0)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/merge-patch+json"] = new() { Schema = patchRef },
                    ["application/json"] = new() { Schema = patchRef },
                },
            };
            return;
        }

        foreach (var media in operation.RequestBody.Content.Values)
        {
            media.Schema = patchRef;
        }
    }

    private static async Task<IOpenApiSchema> BuildPatchSchemaAsync(
        Type clrType,
        JsonTypeInfo typeInfo,
        OpenApiOperationTransformerContext context,
        JsonSerializerOptions serializerOptions,
        JsonMergeOptions mergeOptions,
        Dictionary<Type, string> visited,
        CancellationToken cancellationToken)
    {
        var document = context.Document;
        ArgumentNullException.ThrowIfNull(document);

        if (visited.TryGetValue(clrType, out var existingId))
        {
            return new OpenApiSchemaReference(existingId, document);
        }

        document.Components ??= new OpenApiComponents();

        var componentId = UniqueComponentId(context, $"JsonMergePatch<{clrType.Name}>");
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>(),
            Required = new HashSet<string>(),
            Description = $"JSON merge patch (RFC 7396) document for {clrType.Name}. All properties are optional.",
        };

        // Register before recursing so self-referencing types terminate.
        document.AddComponent(componentId, schema);
        visited[clrType] = componentId;

        foreach (var property in typeInfo.Properties)
        {
            if (!IsPropertyPatchable(property, mergeOptions))
            {
                continue;
            }

            IOpenApiSchema propertySchema;

            JsonTypeInfo? nested = null;
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
                var nestedRef = await BuildPatchSchemaAsync(
                    nested.Type,
                    nested,
                    context,
                    serializerOptions,
                    mergeOptions,
                    visited,
                    cancellationToken).ConfigureAwait(false);
                propertySchema = MakeNullable(nestedRef);
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

            schema.Properties[property.Name] = propertySchema;
        }

        return new OpenApiSchemaReference(componentId, document);
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

    private static string UniqueComponentId(OpenApiOperationTransformerContext context, string baseId)
    {
        var schemas = context.Document?.Components?.Schemas;
        if (schemas is null || !schemas.ContainsKey(baseId))
        {
            return baseId;
        }

        var i = 2;
        while (schemas.ContainsKey($"{baseId}{i}"))
        {
            i++;
        }

        return $"{baseId}{i}";
    }

    private static Type? GetPatchTargetType(OpenApiOperationTransformerContext context)
    {
        var description = context.Description;

        if (description?.ParameterDescriptions is not null)
        {
            foreach (var parameter in description.ParameterDescriptions)
            {
                if (TryGetDocumentTarget(parameter.Type, out var target))
                {
                    return target;
                }

                var descriptorType = parameter.ParameterDescriptor?.ParameterType;
                if (descriptorType is not null && TryGetDocumentTarget(descriptorType, out var target2))
                {
                    return target2;
                }
            }
        }

        var action = description?.ActionDescriptor;
        if (action?.Parameters is not null)
        {
            foreach (var parameter in action.Parameters)
            {
                if (TryGetDocumentTarget(parameter.ParameterType, out var target))
                {
                    return target;
                }
            }
        }

        if (action?.EndpointMetadata is not null)
        {
            foreach (var metadata in action.EndpointMetadata)
            {
                // Minimal-API handlers surface here as MethodInfo (e.g. RuntimeMethodInfo
                // for lambdas). ApiExplorer only reports the AcceptsMetadata body type
                // (usually System.Object), so the handler signature is the reliable
                // source for JsonMergePatchDocument<T>.
                if (metadata is System.Reflection.MethodInfo method)
                {
                    foreach (var handlerParameter in method.GetParameters())
                    {
                        if (TryGetDocumentTarget(handlerParameter.ParameterType, out var target))
                        {
                            return target;
                        }
                    }
                }

                if (metadata is AcceptsMetadata accepts &&
                    accepts.RequestType is not null &&
                    TryGetDocumentTarget(accepts.RequestType, out var target2))
                {
                    return target2;
                }
            }
        }

        return null;
    }

    private static bool TryGetDocumentTarget(Type candidate, out Type? target)
    {
        if (candidate.IsGenericType &&
            candidate.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>))
        {
            target = candidate.GetGenericArguments()[0];
            return true;
        }

        target = null;
        return false;
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
