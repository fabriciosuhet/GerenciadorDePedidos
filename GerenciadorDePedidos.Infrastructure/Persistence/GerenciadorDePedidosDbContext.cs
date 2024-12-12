using System.Reflection;
using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence;

public class GerenciadorDePedidosDbContext : DbContext
{
	public GerenciadorDePedidosDbContext()
	{
	}

	public GerenciadorDePedidosDbContext(DbContextOptions<GerenciadorDePedidosDbContext> options) : base(options)
	{
	}
	
	public DbSet<Cliente> Clientes { get; set; }
	public DbSet<ItemPedido> ItensPedidos { get; set; }
	public DbSet<Pedido> Pedidos { get; set; }
	public DbSet<Produto> Prdutos { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}
}