using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Adv76.AspNetCore.JsonMergePatch;

public static class TypedResultsExtensions
{
    extension(TypedResults)
    {
        public static ValidationProblem ValidationProblem(JsonMergePatchResult result)
        {
            if (result.Succeeded)
            {
                throw new InvalidOperationException("The JsonMergePatch was successful.");
            }

            var errors = result.Errors.ToDictionary<KeyValuePair<string, string>, string, string[]>(
                kvp => kvp.Key,
                kvp => [kvp.Value]);
            
            return TypedResults.ValidationProblem(errors);
        }
    }
}