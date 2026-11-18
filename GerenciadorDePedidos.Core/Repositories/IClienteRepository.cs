using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IClienteRepository : IRepository<Cliente, Guid>
{
	Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default);
	Task<List<Cliente>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default);
	Task<Cliente?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}