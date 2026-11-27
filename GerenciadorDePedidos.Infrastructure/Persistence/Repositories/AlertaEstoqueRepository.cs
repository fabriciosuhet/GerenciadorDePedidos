using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class AlertaEstoqueRepository(GerenciadorDePedidosDbContext context)
	: Repository<AlertaEstoque, int>(context), IAlertaEstoqueRepository
{
	public Task<AlertaEstoque?> GetPendenteByProdutoIdAsync(int produtoId, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Where(a => a.ProdutoId == produtoId && !a.Resolvido)
			.OrderByDescending(a => a.CriadoEm)
			.FirstOrDefaultAsync(cancellationToken);

	public Task<List<AlertaEstoque>> GetAllAsync(bool? apenasPendentes = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.IgnoreQueryFilters().AsNoTracking().AsQueryable();

		if (apenasPendentes == true)
			query = query.Where(a => !a.Resolvido);
		else if (apenasPendentes == false)
			query = query.Where(a => a.Resolvido);

		return query
			.OrderBy(a => a.Resolvido)
			.ThenByDescending(a => a.CriadoEm)
			.ToListAsync(cancellationToken);
	}
}

