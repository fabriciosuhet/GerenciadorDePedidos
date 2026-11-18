using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface ILoginRepository : IRepository<Login, int>
{
	Task<Login?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<Login?> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
