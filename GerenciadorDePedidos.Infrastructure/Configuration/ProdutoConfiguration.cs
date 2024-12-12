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
	}
}