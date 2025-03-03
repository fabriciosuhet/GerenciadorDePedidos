using GerenciadorDePedidos.Core.Entities;
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
			.HasOne(m => m.Produto)
			.WithMany(p => p.MovimentacaoEstoque)
			.HasForeignKey(m => m.ProdutoId);
		
	}
}