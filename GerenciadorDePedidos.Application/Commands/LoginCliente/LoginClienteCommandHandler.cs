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
	IUnitOfWork unitOfWork,
	ILoginAttemptService loginAttemptService) : IRequestHandler<LoginClienteCommand, LoginClienteViewModel?>
{
	// Hash BCrypt sintaticamente válido usado quando o e-mail não existe ou está bloqueado,
	// mitigando enumeração de usuários por diferença de tempo de resposta (timing attack).
	private const string DummyPasswordHash = "$2a$11$CwTycUXWue0Thq9StjUM0uJ8l1Q6sG0T5l6fZ7x9Q2vK9dG5n8M1y";

	public async Task<LoginClienteViewModel?> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
	{
		if (loginAttemptService.IsLockedOut(request.Email))
		{
			authService.VerifyPassword(request.Password, DummyPasswordHash);
			return null;
		}

		var login = await loginRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (login is null)
		{
			authService.VerifyPassword(request.Password, DummyPasswordHash);
			loginAttemptService.RegisterFailedAttempt(request.Email);
			return null;
		}

		var passwordValida = authService.VerifyPassword(request.Password, login.SenhaHash);
		if (!login.Ativo || login.Cliente is { Ativo: false } || !passwordValida)
		{
			loginAttemptService.RegisterFailedAttempt(request.Email);
			return null;
		}

		loginAttemptService.ResetAttempts(request.Email);

		var accessToken = authService.GenerateJwtToken(login.ClienteId, login.Email, login.Role.ToString());
		var refreshToken = authService.GenerateRefreshToken();
		var refreshTokenHash = authService.HashToken(refreshToken);

		await refreshTokenRepository.AddAsync(
			new Core.Entities.RefreshToken(login.Id, refreshTokenHash, authService.GetRefreshTokenExpirationUtc()),
			cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new LoginClienteViewModel(login.Email, accessToken, refreshToken);
	}
}