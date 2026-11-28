namespace GerenciadorDePedidos.Core.Entities;

public class PasswordResetToken : BaseEntity<int>
{
	public string TokenHash { get; private set; } = string.Empty;
	public int LoginId { get; private set; }
	public Login Login { get; private set; } = null!;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	public DateTime ExpiresAt { get; private set; }
	public DateTime? UsedAt { get; private set; }
	public DateTime? RevokedAt { get; private set; }

	public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
	public bool IsUsed => UsedAt is not null;
	public bool IsRevoked => RevokedAt is not null;
	public bool IsActive => !IsUsed && !IsRevoked && !IsExpired;

	protected PasswordResetToken()
	{
	}

	public PasswordResetToken(int loginId, string tokenHash, DateTime expiresAt)
	{
		if (loginId <= 0)
			throw new ArgumentException("O identificador de login é inválido.");

		if (string.IsNullOrWhiteSpace(tokenHash))
			throw new ArgumentException("O hash do token de redefinição não pode ser vazio.");

		LoginId = loginId;
		TokenHash = tokenHash;
		CreatedAt = DateTime.UtcNow;
		ExpiresAt = expiresAt;
	}

	public void MarcarComoUtilizado()
	{
		if (IsUsed)
			throw new InvalidOperationException("O token de redefinição de senha já foi utilizado.");

		if (IsRevoked)
			throw new InvalidOperationException("O token de redefinição de senha foi revogado.");

		if (IsExpired)
			throw new InvalidOperationException("O token de redefinição de senha expirou.");

		UsedAt = DateTime.UtcNow;
	}

	public void Revogar()
	{
		if (IsUsed || IsRevoked)
			return;

		RevokedAt = DateTime.UtcNow;
	}
}

