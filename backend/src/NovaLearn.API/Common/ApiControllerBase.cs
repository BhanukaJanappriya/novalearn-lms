using Microsoft.AspNetCore.Mvc;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Common;

/// <summary>
/// Base controller that translates the Application's <see cref="Result"/> vocabulary into
/// HTTP responses, keeping controllers thin and consistent (RFC 7807 problem details on failure).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<TValue>(Result<TValue> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected IActionResult HandleResult<TValue>(Result<TValue> result, Func<TValue, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess ? NoContent() : Problem(result.Error);

    private ObjectResult Problem(Error error)
    {
        int status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{status}"
        };

        return new ObjectResult(problem) { StatusCode = status };
    }
}
