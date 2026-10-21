
using MedConnect.Models;

namespace MedConnect.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

