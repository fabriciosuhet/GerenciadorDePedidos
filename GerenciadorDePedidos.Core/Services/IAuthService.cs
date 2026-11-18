namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(string email, string role);
	string HashPassword(string password);
	bool VerifyPassword(string password, string passwordHash);
}