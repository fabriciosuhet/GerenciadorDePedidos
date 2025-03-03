using System.Collections;
using System.Globalization;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
{
	private readonly GerenciadorDePedidosDbContext _context;

	public MovimentacaoEstoqueRepository(GerenciadorDePedidosDbContext context) => _context = context;

	public async Task AddAsync(MovimentacaoEstoque movimentacaoEstoque)
	{
		await _context.MovimentacaoEstoques.AddAsync(movimentacaoEstoque);
		await _context.SaveChangesAsync();
	}

	public async Task<IEnumerable<MovimentacaoEstoque>> GetAllAsync(string? query)
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
		
		return await _context.MovimentacaoEstoques
			.Include(me => me.Produto)
			.Where(me =>
			string.IsNullOrEmpty(query)
			|| me.Produto.Nome.Contains(query)
			|| (tipoMovimentacao.HasValue && me.TipoMovimentacao.Equals(tipoMovimentacao.Value))
			|| (dataMovimentacao.HasValue && me.DataMovimentacao.Equals(dataMovimentacao.Value))).ToListAsync();

	}

	public async Task<MovimentacaoEstoque?> GetByIdAsync(Guid id)
	{
		return await _context.MovimentacaoEstoques
			.Include(me => me.Produto)
			.AsNoTracking()
			.SingleOrDefaultAsync(me => me.Id == id);
	}
}