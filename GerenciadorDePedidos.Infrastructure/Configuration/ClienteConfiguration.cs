using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
	public void Configure(EntityTypeBuilder<Cliente> builder)
	{
		builder
			.HasKey(c => c.Id);

		builder
			.HasMany(c => c.Pedidos)
			.WithOne(p => p.Cliente)
			.HasForeignKey(p => p.ClienteId)
			.OnDelete(DeleteBehavior.Restrict);

	}
}