using AuthService.Domain.Entitis;

namespace AuthService.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
