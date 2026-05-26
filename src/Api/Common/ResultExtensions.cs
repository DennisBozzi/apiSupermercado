using ApiBozzis.Shared.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiBozzis.Api.Common;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess) return new NoContentResult();
        return new ObjectResult(ToProblem(result.Error)) { StatusCode = StatusCodeFor(result.Error.Type) };
    }

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        return new ObjectResult(ToProblem(result.Error)) { StatusCode = StatusCodeFor(result.Error.Type) };
    }

    private static ProblemDetails ToProblem(Error error) => new()
    {
        Type = $"https://errors.apibozzis/{error.Code}",
        Title = error.Code,
        Detail = error.Message,
        Status = StatusCodeFor(error.Type),
    };

    private static int StatusCodeFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError,
    };
}
