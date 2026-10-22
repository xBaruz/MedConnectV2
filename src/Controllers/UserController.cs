using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Dtos;
using MedConnect.Core.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class UserController(IQueryHandler<GetCurrentUserQuery, UserResponseDto> handler): ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCurrentUserQuery(), ct);

        return result.Match<IActionResult>(
            success => Ok(success),
            failure => failure switch
            {
                Error.NotFound error => NotFound(
                    new { code = error.Code, message = error.Message }),

                Error.Unauthorized error => Unauthorized(
                    new { code = error.Code, message = error.Message }),

                _ => Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Wystąpił nieoczekiwany błąd.")
            });
    }
}