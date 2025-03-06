using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(string email, string role);
	string ComputeSha256Hash(string password);
	bool IsAuthenticated();
	bool IsInRole(Role role);
}