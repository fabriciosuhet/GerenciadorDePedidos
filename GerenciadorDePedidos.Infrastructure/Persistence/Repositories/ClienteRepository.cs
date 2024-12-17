using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
	private readonly GerenciadorDePedidosDbContext _context;

	public ClienteRepository(GerenciadorDePedidosDbContext context)
	{
		_context = context;
	}

	public async Task<List<Cliente>> GetAllAsync()
	{
		return await _context.Clientes.ToListAsync();
	}

	public async Task<Cliente?> GetByIdAsync(Guid id)
	{
		return await _context.Clientes.SingleOrDefaultAsync(c => c.Id == id);
	}

	public async Task<Cliente?> GetDetailsByIdAsync(Guid id)
	{
		return await _context.Clientes
			.Include(c => c.Pedidos)
			.ThenInclude(p => p.ItensPedidos)
			.SingleOrDefaultAsync(c => c.Id == id);
	}

	public async Task AddAsync(Cliente cliente)
	{
		await _context.AddAsync(cliente);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(Guid id, Cliente cliente)
	{
		var clienteExistente = await _context.Clientes.SingleOrDefaultAsync(c => c.Id == id);
		if (clienteExistente == null)
		{
			throw new Exception("Cliente nao encontrado");
		}
		
		clienteExistente.AlterarEmail(cliente.Email);
		clienteExistente.AlterarTelefone(cliente.Telefone);
		
		await _context.SaveChangesAsync();

	}

	public async Task DeleteAsync(Guid id)
	{
		var clienteExistente = await _context.Clientes.SingleOrDefaultAsync(c => c.Id == id);
		if (clienteExistente is null)
		{
			throw new Exception("Cliente nao encontrado");
		}

		_context.Remove(clienteExistente);
		await _context.SaveChangesAsync();
	}

	public async Task<Cliente?> GetUserByEmailAndPasswordAsync(string email, string passwordHash)
	{
		return await _context.Clientes
			.SingleOrDefaultAsync(c => c.Email.Equals(email) && c.Senha.Equals(passwordHash));
	}
}