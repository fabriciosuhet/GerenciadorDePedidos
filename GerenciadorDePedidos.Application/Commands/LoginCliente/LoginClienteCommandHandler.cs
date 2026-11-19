using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public sealed class LoginClienteCommandHandler(
	IAuthService authService,
	ILoginRepository loginRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<LoginClienteCommand, LoginClienteViewModel?>
{
	public async Task<LoginClienteViewModel?> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
	{
		var login = await loginRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (login is null || !authService.VerifyPassword(request.Password, login.SenhaHash))
			return null;

		var accessToken = authService.GenerateJwtToken(login.ClienteId, login.Email, login.Role.ToString());
		var refreshToken = authService.GenerateRefreshToken();

		await refreshTokenRepository.AddAsync(
			new Core.Entities.RefreshToken(login.Id, refreshToken, authService.GetRefreshTokenExpirationUtc()),
			cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new LoginClienteViewModel(login.Email, accessToken, refreshToken);
	}
}