using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ProdutoRepository : Repository<Produto, int>, IProdutoRepository
{
    public ProdutoRepository(GerenciadorDePedidosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Produto>> GetAllProdutos(string? query)
    {
		return await _context.Produtos
			.AsNoTracking()
			.Where(x => string.IsNullOrEmpty(query) || x.Nome.Contains(query))
			.ToListAsync();
    }

    public async Task<int> GetCountAsync(string? query)
	{
		return await _context.Produtos.AsNoTracking().Where(p => string.IsNullOrEmpty(query) || p.Nome.Contains(query))
			.CountAsync();
	}

	public async Task<List<Produto>> GetPagedAsync(string? query, int skip, int take)
	{
		return await _dbSet
			.Where(p => string.IsNullOrEmpty(query) || p.Nome.Contains(query))
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}
}