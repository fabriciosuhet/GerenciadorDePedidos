namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(Guid clienteId, string email, string role);
	string GenerateRefreshToken();
	string HashToken(string token);
	DateTime GetRefreshTokenExpirationUtc();
	string HashPassword(string password);
	bool VerifyPassword(string password, string passwordHash);
}