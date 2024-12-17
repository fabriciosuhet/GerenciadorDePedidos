namespace GerenciadorDePedidos.Application.Models;

public class LoginClienteViewModel
{
	public string Email { get; private set; }
	public string Token { get; private set; }

	public LoginClienteViewModel(string email, string token)
	{
		Email = email;
		Token = token;
	}
}