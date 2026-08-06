using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Adv76.AspNetCore.JsonMergePatch;

public static class OpenApiExtensions
{
    public static RouteHandlerBuilder AcceptsJsonMergePatch(this RouteHandlerBuilder builder)
        => builder.Accepts<object>("application/merge-patch+json", "application/json");
}