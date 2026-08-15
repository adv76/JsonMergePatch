using Adv76.AspNetCore.JsonMergePatch;
using Adv76.AspNetCore.JsonMergePatch.Test;
using Microsoft.AspNetCore.Http.HttpResults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var class1 = new Class1()
{
    Int1 = 1,
    NullableInt1 = 1,
    String1 = "Hello",
    NullableString1 = "World",
    RequiredString1 = "Required",
    RequiredString2 = "Required2",
    ProhibitedString1 = "Prohibited",
    ReadonlyString1 = "Readonly",
    Class2 = new Class2()
    {
        Int1 = 1,
        NullableInt1 = 1,
        String1 = "Hello",
        NullableString1 = "World",
        RequiredString1 = "Required",
        ProhibitedString1 = "Prohibited"
    }
};

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapGet("/class1", () => class1);

app.MapPatch("/class1", (JsonMergePatchDocument<Class1> doc) =>
{
    doc.ApplyTo(ref class1);
    
    return class1;
}).AcceptsJsonMergePatch();
    
app.MapPatch("/class1/safe", Results<Ok<Class1>, ValidationProblem>(TypedJsonMergePatchDocument<Class1> doc) =>
{
    var result = doc.SafeApplyTo(ref class1);
    if (result.Succeeded)
    {
        return TypedResults.Ok(class1);
    }

    return TypedResults.ValidationProblem(result);
});//.AcceptsTypedJsonMergePatch<Class1>();

app.Run();
