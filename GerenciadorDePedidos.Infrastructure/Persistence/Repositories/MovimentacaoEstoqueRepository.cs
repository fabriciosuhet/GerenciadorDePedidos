using System.Collections;
using System.Globalization;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class MovimentacaoEstoqueRepository : Repository<MovimentacaoEstoque, int>, IMovimentacaoEstoqueRepository
{
	public MovimentacaoEstoqueRepository(GerenciadorDePedidosDbContext context) : base(context)
	{
	}

	public async Task<int> GetCountAsync(string? query)
	{
		DateTime? dataMovimentacao = null;

		if (!string.IsNullOrEmpty(query) && DateTime.TryParse(query, out var parsedDate))
		{
			dataMovimentacao = parsedDate;
		}

		Tipo? tipoMovimentacao = null;

		if (!string.IsNullOrEmpty(query) && Enum.TryParse(query, out Tipo parsedTipo))
		{
			tipoMovimentacao = parsedTipo;
		}

		return await _dbSet.AsNoTracking()
			.Include(me => me.Produto)
			.Where(me =>
				string.IsNullOrEmpty(query) || me.Produto.Nome.Contains(query)
											|| (tipoMovimentacao.HasValue &&
												me.TipoMovimentacao.Equals(tipoMovimentacao.Value))
											|| (dataMovimentacao.HasValue &&
												me.DataMovimentacao.Equals(dataMovimentacao.Value))).CountAsync();

	}

	public async Task<ICollection<MovimentacaoEstoque>> GetPagedAsync(string? query, int skip, int take)
	{
		return await _dbSet.AsNoTracking()
			.Include(me => me.Produto)
			.Where(me => string.IsNullOrEmpty(query) || me.Produto.Nome.Contains(query))
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}

	public async Task<MovimentacaoEstoque?> GetByIdAMovimentacaoAsync(int id)
	{
		return await _dbSet
			.Include(me => me.Produto)
			.Include(m => m.Cliente)
			.AsNoTracking()
			.FirstOrDefaultAsync(me => me.Id == id);
	}

    public async Task<MovimentacaoEstoque?>? GetByIdMovimentacaoAsync(int id)
    {
		return await _dbSet
			.AsNoTracking()
			.Include(me => me.Produto)
			.Include(me => me.Cliente)
			.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MovimentacaoEstoque?>>? GetAllMovimentacaoAsync(string? query)
    {
		return await _dbSet
			.AsNoTracking()
			.Include(me => me.Produto)
			.Include(me => me.Cliente)
			.ToListAsync();
    }
}