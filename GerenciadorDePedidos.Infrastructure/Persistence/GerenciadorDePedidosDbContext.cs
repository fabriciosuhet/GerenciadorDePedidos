using System.Reflection;
using GerenciadorDePedidos.Core.Entities;
using MassTransit;
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
	public DbSet<Produto> Produtos { get; set; }
	public DbSet<MovimentacaoEstoque> MovimentacaoEstoques { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; }
	public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
	public DbSet<AlertaEstoque> AlertasEstoque { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// Tabelas do Transactional Outbox / Inbox do MassTransit
		modelBuilder.AddInboxStateEntity();
		modelBuilder.AddOutboxMessageEntity();
		modelBuilder.AddOutboxStateEntity();
	}

	public override int SaveChanges()
	{
		AplicarAuditoria();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		AplicarAuditoria();
		return base.SaveChangesAsync(cancellationToken);
	}

	private void AplicarAuditoria()
	{
		var agora = DateTime.UtcNow;

		foreach (var entry in ChangeTracker.Entries())
		{
			if (entry.State is not (EntityState.Added or EntityState.Modified))
				continue;

			if (!EhBaseEntity(entry.Entity.GetType()))
				continue;

			if (entry.State == EntityState.Added)
			{
				var criadoEmProp = entry.Property("CriadoEm");
				if (criadoEmProp.CurrentValue is DateTime dt && dt == default)
					criadoEmProp.CurrentValue = agora;
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Property("CriadoEm").IsModified = false;
				entry.Property("AtualizadoEm").CurrentValue = agora;
			}
		}
	}

	private static bool EhBaseEntity(Type type)
	{
		var current = type.BaseType;
		while (current is not null)
		{
			if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseEntity<>))
				return true;

			current = current.BaseType;
		}

		return false;
	}
}