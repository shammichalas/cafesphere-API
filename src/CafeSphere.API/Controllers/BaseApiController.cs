using CafeSphere.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NoContent();

            return Ok(result.Value);
        }

        return MapErrorToResponse(result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return MapErrorToResponse(result);
    }

    private IActionResult MapErrorToResponse(Result result)
    {
        if (result.ValidationErrors.Count > 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = result.Error.Message,
                Extensions = { ["errors"] = result.ValidationErrors }
            });
        }

        return result.Error.Code switch
        {
            "Resource.NotFound" => NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error.Message }),
            "Auth.Unauthorized" => Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = result.Error.Message }),
            "Auth.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Title = "Forbidden", Detail = result.Error.Message }),
            "Resource.Conflict" => Conflict(new ProblemDetails { Title = "Conflict", Detail = result.Error.Message }),
            _ => BadRequest(new ProblemDetails { Title = "Bad Request", Detail = result.Error.Message })
        };
    }
}
