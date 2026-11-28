using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
	public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
	{
		builder.ToTable("PasswordResetTokens");

		builder.HasKey(t => t.Id);

		builder.Property(t => t.TokenHash)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(t => t.UsedAt)
			.IsConcurrencyToken();

		builder.Property(t => t.RevokedAt)
			.IsConcurrencyToken();

		builder.HasIndex(t => t.TokenHash)
			.IsUnique();

		builder.HasIndex(t => new { t.LoginId, t.UsedAt, t.RevokedAt });

		builder.HasOne(t => t.Login)
			.WithMany()
			.HasForeignKey(t => t.LoginId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

