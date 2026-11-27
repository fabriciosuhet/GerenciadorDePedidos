using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class AlertaEstoqueConfiguration : IEntityTypeConfiguration<AlertaEstoque>
{
	public void Configure(EntityTypeBuilder<AlertaEstoque> builder)
	{
		builder.ToTable("AlertasEstoque");
		builder.HasKey(a => a.Id);

		builder.Property(a => a.ProdutoNome)
			.IsRequired()
			.HasMaxLength(200);

		builder.HasOne(a => a.Produto)
			.WithMany()
			.HasForeignKey(a => a.ProdutoId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasIndex(a => new { a.ProdutoId, a.Resolvido });
	}
}

