using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
	IPasswordResetTokenRepository passwordResetTokenRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IAuthService authService,
	ILoginAttemptService loginAttemptService,
	IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Unit>
{
	private const string MensagemTokenInvalido = "Token de redefinição de senha inválido ou expirado.";

	public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
	{
		var tokenHash = authService.HashToken(request.Token.Trim());
		var resetToken = await passwordResetTokenRepository.GetByTokenHashWithLoginAsync(tokenHash, cancellationToken);

		if (resetToken is null
			|| !resetToken.IsActive
			|| !resetToken.Login.Ativo
			|| resetToken.Login.Cliente is { Ativo: false })
		{
			throw new ArgumentException(MensagemTokenInvalido);
		}

		var novaSenhaHash = authService.HashPassword(request.NovaSenha);
		resetToken.Login.AlterarSenha(novaSenhaHash);
		resetToken.MarcarComoUtilizado();

		// Garante que nenhum outro token de redefinição ou sessão (refresh token) anterior permaneça válido.
		await passwordResetTokenRepository.InvalidateAllActiveByLoginIdAsync(resetToken.LoginId, cancellationToken);
		await refreshTokenRepository.RevokeAllActiveByLoginIdAsync(resetToken.LoginId, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		// Limpa eventual bloqueio por tentativas falhas para permitir login imediato com a nova senha.
		loginAttemptService.ResetAttempts(resetToken.Login.Email);

		AppDiagnostics.RedefinicoesSenhaConcluidas.Add(1);

		return Unit.Value;
	}
}
