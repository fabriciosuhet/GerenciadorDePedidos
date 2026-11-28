using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
	ILoginRepository loginRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IPasswordResetTokenRepository passwordResetTokenRepository,
	IAuthService authService,
	ICurrentUserService currentUserService,
	ILoginAttemptService loginAttemptService,
	IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, Unit>
{
	public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
	{
		if (currentUserService.ClienteId is null)
			throw new UnauthorizedAccessException("Usuário não autenticado.");

		var login = await loginRepository.GetByClienteIdAsync(currentUserService.ClienteId.Value, cancellationToken);
		if (login is null || !login.Ativo)
			throw new UnauthorizedAccessException("Conta não encontrada ou desativada.");

		if (!authService.VerifyPassword(request.SenhaAtual, login.SenhaHash))
			throw new ArgumentException("Senha atual inválida.");

		if (authService.VerifyPassword(request.NovaSenha, login.SenhaHash))
			throw new ArgumentException("A nova senha deve ser diferente da senha atual.");

		var novaSenhaHash = authService.HashPassword(request.NovaSenha);
		login.AlterarSenha(novaSenhaHash);

		await refreshTokenRepository.RevokeAllActiveByLoginIdAsync(login.Id, cancellationToken);
		await passwordResetTokenRepository.InvalidateAllActiveByLoginIdAsync(login.Id, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		loginAttemptService.ResetAttempts(login.Email);

		AppDiagnostics.RedefinicoesSenhaConcluidas.Add(1);

		return Unit.Value;
	}
}
