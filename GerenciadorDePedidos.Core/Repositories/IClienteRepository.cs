using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IClienteRepository
{
	Task<List<Cliente>> GetAllAsync(string? query);
	Task<Cliente?> GetByIdAsync(Guid id);
	Task<Cliente?> GetDetailsByIdAsync(Guid id);
	Task AddAsync(Cliente cliente);
	Task UpdateAsync(Guid id, Cliente cliente);
	Task DeleteAsync(Guid id);
	Task<Cliente?> GetUserByEmailAndPasswordAsync(string email, string passwordHash);
}