using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration
{
    public class LoginConfiguration : IEntityTypeConfiguration<Login>
    {
        public void Configure(EntityTypeBuilder<Login> builder)
        {
            builder.Property(l => l.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(l => l.SenhaHash)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(l => l.Email)
                   .IsUnique();

            builder.HasOne(l => l.Cliente)
                 .WithOne()
                 .HasForeignKey<Login>(l => l.ClienteId)
                 .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
