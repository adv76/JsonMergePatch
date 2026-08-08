using Adv76.JsonMergePatch;
using Microsoft.AspNetCore.Http;

namespace Adv76.AspNetCore.JsonMergePatch;

public static class ResultsExtensions
{
    extension(Results)
    {
        public static IResult ValidationProblem(JsonMergePatchResult result)
            => TypedResults.ValidationProblem(result);
    }
}