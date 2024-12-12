using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IProdutoRepository
{
	Task<List<Produto>> GetAllAsync();
	Task<Produto?> GetByIdAsync(Guid id);
	Task AddAsync(Produto produto);
	Task UpdateAsync(Guid id,Produto produto);
	Task DeleteAsync(Guid id);
}