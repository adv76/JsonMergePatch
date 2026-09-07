# Adv76.AspNetCore.JsonMergePatch.OpenApi

OpenAPI integration for using Adv76.JsonMergePatch in ASP.NET Core apps with `Microsoft.OpenAPI`.
This package contains transformers for adding typing for JSON Merge Patch operations to
OpenAPI documentation.


Register it when configuring OpenAPI:

    using Adv76.AspNetCore.JsonMergePatch.OpenApi;

    ...

    builder.Services.AddOpenApi(options => options.AddJsonMergePatch());

Endpoints with body type of `JsonMergePatchDocument<T>` get a merge-patch
request body schema in the OpenAPI document. 
- All properties are optional and nullable.
- Properties block from patching via security policies are hidden.
- Nested objects recurse into their own patch schemas.
