using GerenciadorDePedidos.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorDePedidos.Infrastructure.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
	public void Configure(EntityTypeBuilder<RefreshToken> builder)
	{
		builder.ToTable("RefreshTokens");

		builder.HasKey(rt => rt.Id);

		builder.Property(rt => rt.Token)
			.IsRequired()
			.HasMaxLength(256);

		builder.Property(rt => rt.ReplacedByToken)
			.HasMaxLength(256);

		builder.Property(rt => rt.RevokedAt)
			.IsConcurrencyToken();

		builder.HasIndex(rt => rt.Token)
			.IsUnique();

		builder.HasOne(rt => rt.Login)
			.WithMany()
			.HasForeignKey(rt => rt.LoginId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}