using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Dtos;
using MedConnect.Core.Queries;
using MedConnect.Models;
using MedConnect.Repositories;

namespace MedConnect.Application.Users;

public sealed class GetCurrentUserHandler(
    IUserReadRepository readRepository,
    IUserContext userContext)
    : IQueryHandler<GetCurrentUserQuery, UserResponseDto>
{
    public async Task<Result<UserResponseDto>> HandleAsync(GetCurrentUserQuery query, CancellationToken ct = default)
    {
        Guid userId;

        try
        {
            userId = userContext.Identity.Id;
        }
        catch (UnauthorizedAccessException)
        {
            return Result<UserResponseDto>.Fail(new Error.Unauthorized("Brak poprawnej tożsamości użytkownika."));
        }

        var user = await readRepository.GetByIdAsync(userId, ct);

        if (user is null)
            return Result<UserResponseDto>.Fail(new Error.NotFound("User", userId));

        var dto = new UserResponseDto(
            user.Id,
            user.Email,
            user.Firstname,
            user.Lastname,
            RoleNames.ToName(user.Role),
            user.DateBirth,
            user.CreatedAt,
            user.LastLoginAt,
            user.ProfileImage);

        return Result<UserResponseDto>.Ok(dto);
    }
}