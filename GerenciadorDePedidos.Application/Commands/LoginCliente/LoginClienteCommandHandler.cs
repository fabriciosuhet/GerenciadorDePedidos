using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public class LoginClienteCommandHandler : IRequestHandler<LoginClienteCommand, LoginClienteViewModel>
{
	private readonly IAuthService _authService;
	private readonly ILoginRepository _loginRepository;

    public LoginClienteCommandHandler(IAuthService authService, ILoginRepository loginRepository)
    {
        _authService = authService;
        _loginRepository = loginRepository;
    }

    public async Task<LoginClienteViewModel> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
	{
		// utilizar o mesmo algoritmo para criar o hash dessa senha
		var passwordHash = _authService.ComputeSha256Hash(request.Password);
		
		// buscar no meu banco de dados um cliente que tenha meu email e minha senha em formato de hash
		var cliente = await _loginRepository.GetEmailAndPasswordAsync(request.Email, passwordHash);
		
		// se nao existir, erro de login
		if (cliente == null) return null;

		// se existir, gero o token usando dados do usuário
		var token = _authService.GenerateJwtToken(cliente.Email, cliente.Role.ToString());
		return new LoginClienteViewModel(cliente.Email, token);
	}
}