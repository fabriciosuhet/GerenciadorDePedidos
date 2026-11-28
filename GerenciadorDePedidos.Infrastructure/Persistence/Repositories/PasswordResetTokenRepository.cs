using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository(GerenciadorDePedidosDbContext context)
	: Repository<PasswordResetToken, int>(context), IPasswordResetTokenRepository
{
	public Task<PasswordResetToken?> GetByTokenHashWithLoginAsync(string tokenHash, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Include(t => t.Login)
				.ThenInclude(l => l.Cliente)
			.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

	public async Task InvalidateAllActiveByLoginIdAsync(int loginId, CancellationToken cancellationToken = default)
	{
		var activeTokens = await _dbSet
			.Where(t => t.LoginId == loginId && t.UsedAt == null && t.RevokedAt == null)
			.ToListAsync(cancellationToken);

		foreach (var token in activeTokens)
			token.Revogar();
	}
}

