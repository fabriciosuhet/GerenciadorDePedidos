using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class PedidoRepository : IPedidoRepository
{
	private readonly GerenciadorDePedidosDbContext _context;

	public PedidoRepository(GerenciadorDePedidosDbContext context)
	{
		_context = context;
	}
	
	public async Task<int> GetCountAsync(string? query)
	{
		return await _context.Pedidos.AsNoTracking()
			.Where(p => string.IsNullOrEmpty(query) || p.Status.ToString().Contains(query)).CountAsync();
	}

	public async Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take)
	{
		return await _context.Pedidos.AsNoTracking()
			.Include(p => p.ItensPedidos)
			.ThenInclude(i => i.Produto)
			.Where(p => string.IsNullOrEmpty(query) || p.Status.ToString().Contains(query))
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}

	public async Task<Pedido?> GetByIdAsync(Guid id)
	{
		return await _context.Pedidos
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.Include(p => p.Cliente)
			.AsNoTracking()
			.SingleOrDefaultAsync(p => p.Id == id);
	}

	public async Task AddAsync(Pedido pedido)
	{
		await _context.AddAsync(pedido);
		await _context.SaveChangesAsync();
	}
	
	public async Task DeleteAsync(Guid id)
	{
		var pedido = await _context.Pedidos.SingleOrDefaultAsync(p => p.Id == id);
		if (pedido is null)
		{
			throw new Exception("O pedido nao foi encontrado");
		}
		_context.Remove(pedido);
		await _context.SaveChangesAsync();
	}
}