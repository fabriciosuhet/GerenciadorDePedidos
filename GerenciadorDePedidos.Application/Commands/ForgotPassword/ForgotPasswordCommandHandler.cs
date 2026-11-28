using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
	ILoginRepository loginRepository,
	IPasswordResetTokenRepository passwordResetTokenRepository,
	IAuthService authService,
	IEmailService emailService,
	IUnitOfWork unitOfWork) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordViewModel>
{
	public const string MensagemGenerica =
		"Se o e-mail informado estiver cadastrado, você receberá as instruções para redefinição de senha.";

	public async Task<ForgotPasswordViewModel> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
	{
		var emailNormalizado = request.Email.Trim();
		var login = await loginRepository.GetByEmailAsync(emailNormalizado, cancellationToken);

		// Prevenção de enumeração de usuários (OWASP): resposta sempre idêntica mesmo quando o e-mail não existe ou está inativo.
		if (login is null || !login.Ativo || login.Cliente is { Ativo: false })
			return new ForgotPasswordViewModel(MensagemGenerica);

		await passwordResetTokenRepository.InvalidateAllActiveByLoginIdAsync(login.Id, cancellationToken);

		var rawToken = authService.GeneratePasswordResetToken();
		var tokenHash = authService.HashToken(rawToken);
		var expiresAtUtc = authService.GetPasswordResetTokenExpirationUtc();

		await passwordResetTokenRepository.AddAsync(
			new PasswordResetToken(login.Id, tokenHash, expiresAtUtc),
			cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		AppDiagnostics.RedefinicoesSenhaSolicitadas.Add(1);

		try
		{
			var recipientName = string.IsNullOrWhiteSpace(login.Cliente?.NomeCompleto)
				? login.Email
				: login.Cliente.NomeCompleto;

			await emailService.SendPasswordResetEmailAsync(
				login.Email,
				recipientName,
				rawToken,
				expiresAtUtc,
				cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch
		{
			// Falhas de envio SMTP já são registradas no IEmailService; não propagar para evitar enumeração de e-mails.
		}

		return new ForgotPasswordViewModel(MensagemGenerica);
	}
}
