using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

public class AuthController : BaseApiController
{
    /// <summary>
    /// Register a new customer or staff user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FullName,
            request.PhoneNumber
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticate user credentials and return JWT Access + Refresh token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginUserQuery(request.EmailOrUsername, request.Password);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Request password reset verification link.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Reset password using valid verification token.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
