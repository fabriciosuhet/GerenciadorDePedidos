using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
	public void Configure(EntityTypeBuilder<Pedido> builder)
	{
		builder
			.HasKey(p => p.Id);
		
		builder
			.HasOne(p => p.Cliente)
			.WithMany(c => c.Pedidos)
			.HasForeignKey(p => p.ClienteId)
			.OnDelete(DeleteBehavior.Restrict);
    }
}