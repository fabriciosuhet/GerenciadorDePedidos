using System.Net;
using System.Net.Mail;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GerenciadorDePedidos.Infrastructure.Email;

public sealed class SmtpEmailService(
	IOptions<EmailOptions> options,
	ILogger<SmtpEmailService> logger) : IEmailService
{
	public async Task SendPasswordResetEmailAsync(
		string destinationEmail,
		string recipientName,
		string resetToken,
		DateTime expiresAtUtc,
		CancellationToken cancellationToken = default)
	{
		var cfg = options.Value;

		if (!cfg.Enabled)
		{
			logger.LogWarning(
				"Envio de e-mail desabilitado (Email:Enabled=false). E-mail de redefinição de senha não foi enviado. Configure a seção Email para habilitar entregas SMTP.");
			return;
		}

		if (string.IsNullOrWhiteSpace(cfg.SmtpHost))
			throw new InvalidOperationException("Email:SmtpHost deve ser configurado quando Email:Enabled estiver ativo.");

		var encodedName = WebUtility.HtmlEncode(recipientName);
		var encodedToken = WebUtility.HtmlEncode(resetToken);
		var expiresFormatted = expiresAtUtc.ToString("dd/MM/yyyy HH:mm 'UTC'");

		var resetLinkHtml = string.Empty;
		if (!string.IsNullOrWhiteSpace(cfg.FrontendResetPasswordUrl))
		{
			var separator = cfg.FrontendResetPasswordUrl.Contains('?') ? "&" : "?";
			var resetLink = $"{cfg.FrontendResetPasswordUrl}{separator}token={Uri.EscapeDataString(resetToken)}";
			var encodedLink = WebUtility.HtmlEncode(resetLink);
			resetLinkHtml = $"""
				<p>Clique no link abaixo para definir uma nova senha:</p>
				<p><a href="{encodedLink}">Redefinir minha senha</a></p>
				""";
		}

		var body = $"""
			<!DOCTYPE html>
			<html lang="pt-BR">
			<body style="font-family: Arial, sans-serif; color: #222; line-height: 1.5;">
				<h2>Redefinição de senha - Gerenciador de Pedidos</h2>
				<p>Olá, {encodedName}.</p>
				<p>Recebemos uma solicitação para redefinir a senha da sua conta.</p>
				{resetLinkHtml}
				<p>Caso prefira informar o token manualmente, utilize o código abaixo:</p>
				<p style="font-family: monospace; font-size: 15px; background: #f4f4f4; padding: 10px; border-radius: 4px;">{encodedToken}</p>
				<p>Este token é de uso único e expira em <strong>{expiresFormatted}</strong>.</p>
				<p>Se você não solicitou esta alteração, ignore este e-mail com segurança.</p>
			</body>
			</html>
			""";

		using var message = new MailMessage
		{
			From = new MailAddress(cfg.FromAddress, cfg.FromName),
			Subject = "Redefinição de senha - Gerenciador de Pedidos",
			Body = body,
			IsBodyHtml = true
		};
		message.To.Add(new MailAddress(destinationEmail));

		using var smtpClient = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
		{
			EnableSsl = cfg.UseSsl,
			DeliveryMethod = SmtpDeliveryMethod.Network
		};

		if (!string.IsNullOrWhiteSpace(cfg.Username))
		{
			smtpClient.UseDefaultCredentials = false;
			smtpClient.Credentials = new NetworkCredential(cfg.Username, cfg.Password);
		}

		try
		{
			await smtpClient.SendMailAsync(message, cancellationToken);
			logger.LogInformation("E-mail de redefinição de senha enviado com sucesso via SMTP.");
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Falha ao enviar e-mail de redefinição de senha via SMTP ({Host}:{Port}).", cfg.SmtpHost, cfg.SmtpPort);
			throw;
		}
	}
}
