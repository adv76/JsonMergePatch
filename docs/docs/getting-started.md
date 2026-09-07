# Getting Started

## Installation

For basic JSON Merge Patch support install `Adv76.JsonMergePatch` from nuget.org.

For integrating with ASP.NET Core, install `Adv76.JsonMergePatch.AspNetCore` from nuget.org.

For customizing OpenAPI documentation, also install `Adv76.JsonMergePatch.AspNetCore.OpenAPI` from nuget.org.

## Usage

### Basic

Suppose you have `Class1` below:

    public class Class1
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
    }
    
And suppose you have this instance:

    var obj = new Class1()
    {
        Int0 = 1,
        String0 = "Hello World",
        Double0 = 3.5,
    };
    
A simple example of patching this object:

    var result = JsonMergePatcher.SafeApplyTo(ref obj, """
    {
        "Int0": 42,
        "String0": "Sphinx of black quartz, judge my vow."
    }
    """);
    
If the patch succeeded, `result.Succeeded` will equal `true` and `obj` will have the `"Int0"` and `"String0"` properties updated to the corresponding values.

### ASP.NET Core

For ASP.NET Core, there are a few additional helpers.

First an example of a basic patch endpoint (Minimal API):

    app.MapPatch("/class1", Results<Ok<Class1>, ValidationProblem>(JsonMergePatchDocument<Class1> doc) =>
    {
        var result = doc.SafeApplyTo(ref class1);
        if (result.Succeeded)
        {
            return TypedResults.Ok(class1);
        }

        return TypedResults.ValidationProblem(result);
    })
    
A couple of things to note from the snippet above:

1. `JsonMergePatchDocument<T>` is a wrapper class that automatically populates the patch document from the HTTP Request body. Calling `SafeApplyTo()` on the patch document internally calls the `JsonMergePatcher.SafeApplyTo()` method.
2. `TypedResults` is extended with an additional overload for `TypedResults.ValidationProblem()` that accepts a `JsonMergePatchResult` for returning errors to the user.

### OpenAPI Integration

Generate strongly-typed OpenAPI documents with the OpenAPI integration. By default, the OpenAPI document just shows an empty object for the body of patch endpoints.

Register the transformer when configuring OpenAPI:

    using Adv76.AspNetCore.JsonMergePatch.OpenApi;

    ...

    builder.Services.AddOpenApi(options => options.AddJsonMergePatch());

Endpoints with body type of `JsonMergePatchDocument<T>` get a merge-patch request body schema in the OpenAPI document.
- All properties are optional and nullable.
- Properties block from patching via security policies are hidden.
- Nested objects recurse into their own patch schemas.
