namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(string email, string role);
	string GenerateRefreshToken();
	DateTime GetRefreshTokenExpirationUtc();
	string HashPassword(string password);
	bool VerifyPassword(string password, string passwordHash);
}