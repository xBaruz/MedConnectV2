using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    ICommandHandler<RegisterUserCommand, AuthResponseDto> registerHandler,
    ICommandHandler<LoginCommand, AuthResponseDto> loginHandler,
    ICommandHandler<RefreshTokenCommand, AuthResponseDto> refreshHandler)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await registerHandler.HandleAsync(command, ct);

        return result.Match<IActionResult>(
            success => Ok(success),
            failure => failure switch
            {
                Error.Validation val => BadRequest(new { message = val.Message }),
                Error.Conflict conf => Conflict(new { message = conf.Message }),
                _ => StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd." })
            }
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await loginHandler.HandleAsync(command, ct);

        return result.Match<IActionResult>(
            success => Ok(success),
            failure => failure switch
            {
                Error.Validation val => BadRequest(new { message = val.Message }),
                Error.Unauthorized unauth => Unauthorized(new { message = unauth.Message }),
                _ => StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd." })
            }
        );
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await refreshHandler.HandleAsync(command, ct);

        return result.Match<IActionResult>(
            success => Ok(success),
            failure => failure switch
            {
                Error.Validation val => BadRequest(new { message = val.Message }),
                Error.Unauthorized unauth => Unauthorized(new { message = unauth.Message }),
                _ => StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd." })
            }
        );
    }
}