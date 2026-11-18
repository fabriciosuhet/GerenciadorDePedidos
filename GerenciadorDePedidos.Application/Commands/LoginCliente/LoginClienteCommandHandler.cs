using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public sealed class LoginClienteCommandHandler(
	IAuthService authService,
	ILoginRepository loginRepository) : IRequestHandler<LoginClienteCommand, LoginClienteViewModel?>
{
	public async Task<LoginClienteViewModel?> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
	{
		var login = await loginRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (login is null || !authService.VerifyPassword(request.Password, login.SenhaHash))
			return null;

		var token = authService.GenerateJwtToken(login.Email, login.Role.ToString());
		return new LoginClienteViewModel(login.Email, token);
	}
}