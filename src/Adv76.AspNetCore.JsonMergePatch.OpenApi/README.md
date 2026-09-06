# Adv76.AspNetCore.JsonMergePatch.OpenApi

OpenAPI integration for using Adv76.JsonMergePatch in ASP.NET Core apps.

Register it when configuring OpenAPI:

    builder.Services.AddOpenApi(options => options.AddJsonMergePatch());

Endpoints whose body type is `JsonMergePatchDocument<T>` get a merge-patch
request body schema in the OpenAPI document: all properties are optional and
nullable, properties blocked via `JsonMergePropertySecurityAttribute` (or the
`JsonMergeOptions.SecurityPolicy` default) are hidden, and nested objects recurse
into their own patch schemas.
