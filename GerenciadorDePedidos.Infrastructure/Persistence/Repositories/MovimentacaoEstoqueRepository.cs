using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class MovimentacaoEstoqueRepository(GerenciadorDePedidosDbContext context)
	: Repository<MovimentacaoEstoque, int>(context), IMovimentacaoEstoqueRepository
{
	public Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query).CountAsync(cancellationToken);

	public Task<List<MovimentacaoEstoque>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query)
			.Include(me => me.Cliente)
			.OrderByDescending(me => me.DataMovimentacao)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<MovimentacaoEstoque?> GetByIdMovimentacaoAsync(int id, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Include(me => me.Produto)
			.Include(me => me.Cliente)
			.SingleOrDefaultAsync(me => me.Id == id, cancellationToken);

	public Task<List<MovimentacaoEstoque>> GetByPeriodoAsync(
		DateTime dataInicio,
		DateTime dataFimExclusiva,
		CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Include(me => me.Produto)
			.Include(me => me.Cliente)
			.Where(me => me.DataMovimentacao >= dataInicio && me.DataMovimentacao < dataFimExclusiva)
			.OrderByDescending(me => me.DataMovimentacao)
			.ToListAsync(cancellationToken);

	private IQueryable<MovimentacaoEstoque> BuildFilteredQuery(string? query)
	{
		var q = _dbSet.IgnoreQueryFilters().AsNoTracking().Include(me => me.Produto).AsQueryable();

		if (string.IsNullOrWhiteSpace(query))
			return q;

		var hasDate = DateTime.TryParse(query, out var parsedDate);
		var hasTipo = Enum.TryParse<Tipo>(query, ignoreCase: true, out var parsedTipo);

		return q.Where(me =>
			me.Produto.Nome.Contains(query)
			|| (hasTipo && me.TipoMovimentacao == parsedTipo)
			|| (hasDate && me.DataMovimentacao.Date == parsedDate.Date));
	}
}