using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IProdutoRepository
{
	Task<int> GetCountAsync(string? query);
	Task<IEnumerable<Produto>> GetAllProdutos(string? query);
	Task<List<Produto>> GetPagedAsync(string? query, int skip, int take);
}