using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IPedidoRepository : IRepository<Pedido, int>
{
	Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default);
	Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default);
	Task<Pedido?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default);
	Task<Pedido?> GetByIdWithItensAsync(int id, CancellationToken cancellationToken = default);
}