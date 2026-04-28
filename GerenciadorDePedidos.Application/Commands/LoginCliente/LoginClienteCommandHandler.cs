using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
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
		string passwordHash = _authService.HashPassowrd(request.Password);
		
		var cliente = await _loginRepository.GetEmailAndPasswordAsync(request.Email);
		
		if (cliente == null) return null;
		
		var isPasswordValid =  _authService.VerifyPassowrd(request.Password, cliente.SenhaHash);

		if (!isPasswordValid)
		{
			return null;
		}
		
		var token = _authService.GenerateJwtToken(cliente.Email, cliente.Role.ToString());
		return new LoginClienteViewModel(cliente.Email, token);
	}
}