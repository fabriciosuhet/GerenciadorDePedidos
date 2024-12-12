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

	public async Task<List<Produto>> GetAllAsync()
	{
		return await _context.Produtos.ToListAsync();
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
		
		produto.AlterarNome(produtoExistente.Nome);
		produto.AlterarPreco(produtoExistente.Preco);
		produto.AdicionarEstoque(produtoExistente.Estoque);
		
	}

	public Task DeleteAsync(Guid id)
	{
		throw new NotImplementedException();
	}
}