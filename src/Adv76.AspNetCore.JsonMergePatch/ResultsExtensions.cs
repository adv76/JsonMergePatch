using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;

namespace Adv76.AspNetCore.JsonMergePatch;

/// <summary>
/// Additional helper methods on <see cref="Results"/>.
/// </summary>
public static class ResultsExtensions
{
    extension(Results)
    {
        /// <summary>
        /// Creates a Problem Details response containing the merge patch errors.
        /// </summary>
        /// <param name="result">The result of the merge patch operation.</param>
        /// <returns>ValidationProblem result object</returns>
        /// <exception cref="InvalidOperationException">Throws if the patch result is not a failure.</exception>
        public static IResult ValidationProblem(JsonMergePatchResult result)
            => TypedResults.ValidationProblem(result);
    }
}