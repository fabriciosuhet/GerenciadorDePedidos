using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ClienteRepository : Repository<Cliente, Guid>, IClienteRepository
{

    public ClienteRepository(GerenciadorDePedidosDbContext context) : base(context)
	{
    }

    public async Task<Cliente?> GetDetailsByIdAsync(Guid id)
	{
		return await _dbSet
			.Include(c => c.Pedidos)
			.ThenInclude(p => p.ItensPedidos)
			.ThenInclude(ip => ip.Produto)
			.AsNoTracking()
			.SingleOrDefaultAsync(c => c.Id == id);
	}

	public async Task<Cliente?> GetUserByEmailAndPasswordAsync(string email, string passwordHash)
	{
		return await _dbSet
			.SingleOrDefaultAsync(c => c.Email == email && c.Senha == passwordHash);
	}
}