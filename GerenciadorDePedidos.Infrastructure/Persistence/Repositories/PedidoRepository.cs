using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class PedidoRepository : Repository<Pedido, int> , IPedidoRepository
{

	public PedidoRepository(GerenciadorDePedidosDbContext context) : base(context)
    {
		
	}
	
	public async Task<int> GetCountAsync(string? query)
	{
		return await _dbSet.AsNoTracking()
			.Where(p => string.IsNullOrEmpty(query) || p.Status.ToString().Contains(query)).CountAsync();
	}

	public async Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take)
	{
		return await _dbSet.AsNoTracking()
			.Include(p => p.ItensPedidos)
			.ThenInclude(i => i.Produto)
			.Where(p => string.IsNullOrEmpty(query) || p.Status.ToString().Contains(query))
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}

	public async Task<Pedido?> GetByIdDetailsAsync(int id)
	{
		return await _dbSet
			.Include(p => p.ItensPedidos)
			.ThenInclude(i => i.Produto)
			.Include(p => p.Cliente)
			.AsNoTracking()
			.SingleOrDefaultAsync(p => p.Id == id);
	}
}