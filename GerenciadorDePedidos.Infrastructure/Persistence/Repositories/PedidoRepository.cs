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
	
	public async Task<List<Pedido>> GetAllAsync(string? query)
	{
		return await _context.Pedidos.ToListAsync();
	}

	public async Task<Pedido?> GetByIdAsync(Guid id)
	{
		return await _context.Pedidos.SingleOrDefaultAsync(p => p.Id == id);
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