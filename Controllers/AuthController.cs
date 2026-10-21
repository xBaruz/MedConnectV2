using MedConnect.Core;
using MedConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await authService.RegisterAsync(command);

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
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await authService.LoginAsync(command);

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
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await authService.RefreshTokenAsync(command);

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