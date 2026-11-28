namespace GerenciadorDePedidos.Core.Services;

public interface IEmailService
{
	Task SendPasswordResetEmailAsync(
		string destinationEmail,
		string recipientName,
		string resetToken,
		DateTime expiresAtUtc,
		CancellationToken cancellationToken = default);
}

