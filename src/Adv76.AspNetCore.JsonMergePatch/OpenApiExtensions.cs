using System.Reflection;
using System.Reflection.Emit;
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

public static class OpenApiExtensions
{
    public static RouteHandlerBuilder AcceptsJsonMergePatch(this RouteHandlerBuilder builder)
        => builder.Accepts<object>("application/merge-patch+json", "application/json");

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