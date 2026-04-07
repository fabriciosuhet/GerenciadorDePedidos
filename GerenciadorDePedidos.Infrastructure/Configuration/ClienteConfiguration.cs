using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
	public void Configure(EntityTypeBuilder<Cliente> builder)
	{
        builder.Property(c => c.NomeCompleto)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(c => c.Telefone)
               .HasMaxLength(20);

        builder.HasMany(c => c.Pedidos)
               .WithOne(p => p.Cliente)
               .HasForeignKey(p => p.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.MovimentacaoEstoque)
               .WithOne()
               .OnDelete(DeleteBehavior.Cascade);
    }
}