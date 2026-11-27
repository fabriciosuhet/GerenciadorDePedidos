using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
	public void Configure(EntityTypeBuilder<Produto> builder)
	{
		builder
			.HasKey(p => p.Id);

		builder
			.Property(p => p.Preco)
			.HasColumnType("decimal(15,2)");

		builder
			.Property(p => p.EstoqueMinimo)
			.HasDefaultValue(Produto.EstoqueMinimoPadrao);

		builder
			.Property(p => p.Versao)
			.IsConcurrencyToken()
			.HasDefaultValue(1);

		builder.HasQueryFilter(p => p.Ativo);
	}
}