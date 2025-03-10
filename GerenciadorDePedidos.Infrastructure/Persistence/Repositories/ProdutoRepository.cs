using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ProdutoRepository : IProdutoRepository
{
	private readonly GerenciadorDePedidosDbContext _context;

	public ProdutoRepository(GerenciadorDePedidosDbContext context)
	{
		_context = context;
	}

	public async Task<int> GetCountAsync(string? query)
	{
		return await _context.Produtos.Where(p => string.IsNullOrEmpty(query) || p.Nome.Contains(query))
			.CountAsync();
	}

	public async Task<List<Produto>> GetPagedAsync(string? query, int skip, int take)
	{
		return await _context.Produtos
			.Where(p => string.IsNullOrEmpty(query) || p.Nome.Contains(query))
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}

	public async Task<Produto?> GetByIdAsync(Guid id)
	{
		return await _context.Produtos.SingleOrDefaultAsync(p => p.Id == id);
	}

	public async Task AddAsync(Produto produto)
	{
		await _context.Produtos.AddAsync(produto);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(Guid id, Produto produto)
	{
		var produtoExistente = await _context.Produtos.SingleOrDefaultAsync(p => p.Id == id);
		if (produtoExistente is null)
		{
			throw new Exception("Produto nao encontrado");
		}
		
		await _context.SaveChangesAsync();
		
	}

	public async Task DeleteAsync(Guid id)
	{
		var produto = await _context.Produtos.SingleOrDefaultAsync(p => p.Id == id);
		if (produto is null)
		{
			throw new Exception("Produto nao encontrado");
		}

		_context.Remove(produto);
		await _context.SaveChangesAsync();
	}
}