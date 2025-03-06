using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IProdutoRepository
{
	Task<int> GetCountAsync(string? query);
	Task<List<Produto>> GetPagedAsync(string? query, int skip, int take);
	Task<Produto?> GetByIdAsync(Guid id);
	Task AddAsync(Produto produto);
	Task UpdateAsync(Guid id,Produto produto);
	Task DeleteAsync(Guid id);
}