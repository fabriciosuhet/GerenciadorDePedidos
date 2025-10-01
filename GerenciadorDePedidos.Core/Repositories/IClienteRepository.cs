using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IClienteRepository
{
	Task<Cliente?> GetDetailsByIdAsync(Guid id);	
	Task<Cliente?> GetUserByEmailAndPasswordAsync(string email, string passwordHash);
}