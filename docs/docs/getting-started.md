# Getting Started

## Installation

For basic JSON Merge Patch support install `Adv76.JsonMergePatch` from nuget.org.

> [!NOTE]
> This package is not on nuget yet. The docs are in progress. For now you will have to clone and build yourself.

For integrating with ASP.NET Core, install `Adv76.JsonMergePatch.AspNetCore` from nuget.org.

> [!NOTE]
> This package is not on nuget yet. The docs are in progress. For now you will have to clone and build yourself.

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
    }).AcceptsJsonMergePatch();
    
A few things to note from the snippet above:

1. `JsonMergePatchDocument<T>` is a wrapper class that automatically populates the patch document from the HTTP Request body. Calling `SafeApplyTo()` on the patch document internally calls the `JsonMergePatcher.SafeApplyTo()` method.
2. `TypedResults` is extended with an additional overload for `TypedResults.ValidationProblem()` that accepts a `JsonMergePatchResult` for returning errors to the user.
3. `AcceptsJsonMergePatch()` sets the endpoint to accept either "application/merge-patch+json" or as a fallback "application/json" as the Content-Type. It also adds the appropriate metadata so the OpenAPI spec is correct.

