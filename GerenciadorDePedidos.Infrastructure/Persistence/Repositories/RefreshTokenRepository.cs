using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(GerenciadorDePedidosDbContext context)
	: Repository<RefreshToken, int>(context), IRefreshTokenRepository
{
	public Task<RefreshToken?> GetByTokenWithLoginAsync(string token, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Include(rt => rt.Login)
				.ThenInclude(l => l.Cliente)
			.SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);

	public async Task RevokeAllActiveByLoginIdAsync(int loginId, CancellationToken cancellationToken = default)
	{
		var activeTokens = await _dbSet
			.Where(rt => rt.LoginId == loginId && rt.RevokedAt == null)
			.ToListAsync(cancellationToken);

		foreach (var token in activeTokens)
			token.Revogar();
	}
}