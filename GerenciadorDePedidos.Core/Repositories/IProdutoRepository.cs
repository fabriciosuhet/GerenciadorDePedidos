using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IProdutoRepository : IRepository<Produto, int>
{
	Task<List<Produto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
	Task<List<Produto>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
	Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default);
	Task<List<Produto>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default);
}