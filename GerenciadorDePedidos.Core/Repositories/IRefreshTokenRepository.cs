using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, int>
{
	Task<RefreshToken?> GetByTokenWithLoginAsync(string token, CancellationToken cancellationToken = default);
	Task RevokeAllActiveByLoginIdAsync(int loginId, CancellationToken cancellationToken = default);
}