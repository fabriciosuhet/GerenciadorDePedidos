using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IClienteRepository
{
	Task<int> GetCountAsync(string? query);
	Task<ICollection<Cliente>> GetPagedAsync(string? query, int skip, int take);
	Task<Cliente?> GetByIdAsync(Guid id);
	Task<Cliente?> GetDetailsByIdAsync(Guid id);
	Task AddAsync(Cliente cliente);
	Task UpdateAsync(Guid id, Cliente cliente);
	Task DeleteAsync(Guid id);
	Task<Cliente?> GetUserByEmailAndPasswordAsync(string email, string passwordHash);
}