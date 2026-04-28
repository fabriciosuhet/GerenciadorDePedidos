using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(string email, string role);
	string HashPassowrd(string password);
	bool VerifyPassowrd(string password, string passwordHash);
	bool IsAuthenticated();
	bool IsInRole(Role role);
}