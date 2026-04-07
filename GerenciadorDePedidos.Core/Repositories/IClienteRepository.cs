using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IClienteRepository : IRepository<Cliente, Guid>
{
	Task<Cliente?> GetDetailsByIdAsync(Guid id);	
}