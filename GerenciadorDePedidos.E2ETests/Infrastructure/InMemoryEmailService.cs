using System.Collections.Concurrent;
using GerenciadorDePedidos.Core.Services;

namespace GerenciadorDePedidos.E2ETests.Infrastructure;

public sealed record SentPasswordResetEmail(
	string DestinationEmail,
	string RecipientName,
	string ResetToken,
	DateTime ExpiresAtUtc);

public sealed class InMemoryEmailService : IEmailService
{
	private readonly ConcurrentQueue<SentPasswordResetEmail> _sentEmails = new();

	public IReadOnlyCollection<SentPasswordResetEmail> SentEmails => _sentEmails.ToArray();

	public Task SendPasswordResetEmailAsync(
		string destinationEmail,
		string recipientName,
		string resetToken,
		DateTime expiresAtUtc,
		CancellationToken cancellationToken = default)
	{
		_sentEmails.Enqueue(new SentPasswordResetEmail(destinationEmail, recipientName, resetToken, expiresAtUtc));
		return Task.CompletedTask;
	}
}

