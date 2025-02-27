using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
	public void Configure(EntityTypeBuilder<ItemPedido> builder)
	{
		builder
			.HasKey(ip => ip.Id);
		
		builder
			.HasOne(ip => ip.Produto)
			.WithMany()
			.HasForeignKey(ip => ip.ProdutoId)
			.OnDelete(DeleteBehavior.Restrict);

		builder
			.Property(p => p.PrecoUnitario)
			.HasColumnType("decimal(15, 2)");
	}
}