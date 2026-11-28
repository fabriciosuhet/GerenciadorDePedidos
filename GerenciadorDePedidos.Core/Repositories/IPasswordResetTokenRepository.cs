using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, int>
{
	Task<PasswordResetToken?> GetByTokenHashWithLoginAsync(string tokenHash, CancellationToken cancellationToken = default);
	Task InvalidateAllActiveByLoginIdAsync(int loginId, CancellationToken cancellationToken = default);
}

