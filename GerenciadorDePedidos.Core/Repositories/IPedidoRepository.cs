using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IPedidoRepository
{
	Task<List<Pedido>> GetAllAsync(string? query);
	Task<Pedido?> GetByIdAsync(Guid id);
	Task AddAsync(Pedido pedido);
	Task DeleteAsync(Guid id);
}