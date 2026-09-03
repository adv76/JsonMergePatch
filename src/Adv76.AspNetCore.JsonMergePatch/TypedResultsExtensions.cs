using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Adv76.AspNetCore.JsonMergePatch;

/// <summary>
/// Additional helper methods on <see cref="TypedResults"/>.
/// </summary>
public static class TypedResultsExtensions
{
    extension(TypedResults)
    {
        /// <summary>
        /// Creates a Problem Details response containing the merge patch errors.
        /// </summary>
        /// <param name="result">The result of the merge patch operation.</param>
        /// <returns>ValidationProblem result object</returns>
        /// <exception cref="InvalidOperationException">Throws if the patch result is not a failure.</exception>
        public static ValidationProblem ValidationProblem(JsonMergePatchResult result)
        {
            if (result.Succeeded)
            {
                throw new InvalidOperationException("The JsonMergePatch was successful.");
            }

            return TypedResults.ValidationProblem(result.Errors);
        }
    }
}