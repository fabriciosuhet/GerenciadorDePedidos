namespace GerenciadorDePedidos.Core.Entities;

public class RefreshToken : BaseEntity<int>
{
	public string Token { get; private set; } = string.Empty;
	public int LoginId { get; private set; }
	public Login Login { get; private set; } = null!;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	public DateTime ExpiresAt { get; private set; }
	public DateTime? RevokedAt { get; private set; }
	public string? ReplacedByToken { get; private set; }

	public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
	public bool IsRevoked => RevokedAt is not null;
	public bool IsActive => !IsRevoked && !IsExpired;

	protected RefreshToken()
	{
	}

	public RefreshToken(int loginId, string token, DateTime expiresAt)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("O refresh token não pode ser vazio.");

		LoginId = loginId;
		Token = token;
		CreatedAt = DateTime.UtcNow;
		ExpiresAt = expiresAt;
	}

	public void Revogar(string? replacedByToken = null)
	{
		if (IsRevoked)
			return;

		RevokedAt = DateTime.UtcNow;
		ReplacedByToken = replacedByToken;
	}
}