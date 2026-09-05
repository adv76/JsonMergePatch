# Adv76.AspNetCore.JsonMergePatch

Integration package for using Adv76.JsonMergePatch in ASP.NET Core apps

An example of a basic patch endpoint (Minimal API):

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
