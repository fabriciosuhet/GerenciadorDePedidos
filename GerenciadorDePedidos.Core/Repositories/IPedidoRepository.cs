using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IPedidoRepository
{
	Task<int> GetCountAsync(string? query);
	Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take);
	Task<Pedido?> GetByIdDetailsAsync(int id);
}