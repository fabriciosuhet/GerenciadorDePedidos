namespace GerenciadorDePedidos.Core.Services;

public interface IAuthService
{
	string GenerateJwtToken(string email, string role);
}