using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
	public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
	{
		builder
			.HasKey(m => m.Id);

		builder
			.Property(m => m.PrecoUnitario)
			.HasColumnType("decimal(15,2)");

		builder
			.Property(m => m.Motivo)
			.HasConversion<int>()
			.HasDefaultValue(MotivoMovimentacao.AjusteManual);

		builder
			.HasOne(m => m.Produto)
			.WithMany(p => p.MovimentacaoEstoque)
			.HasForeignKey(m => m.ProdutoId)
			.OnDelete(DeleteBehavior.Restrict);

		builder
			.HasOne(m => m.Cliente)
			.WithMany(c => c.MovimentacaoEstoque)
			.HasForeignKey(m => m.ClienteId)
			.OnDelete(DeleteBehavior.Restrict);

		builder
			.HasOne(m => m.Pedido)
			.WithMany()
			.HasForeignKey(m => m.PedidoId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}