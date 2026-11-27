using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorDePedidos.Infrastructure.Auth;

public class AuthService(IConfiguration configuration) : IAuthService
{
	public string GenerateJwtToken(Guid clienteId, string email, string role)
	{
		var issuer = configuration["Jwt:Issuer"];
		var audience = configuration["Jwt:Audience"];
		var key = configuration["Jwt:Key"];
		if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
			throw new InvalidOperationException("Jwt:Key não configurada ou com tamanho inferior a 32 bytes.");

		var expirationMinutes = configuration.GetValue("Jwt:AccessTokenExpirationMinutes", 15);
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, clienteId.ToString()),
			new("clienteId", clienteId.ToString()),
			new("userName", email),
			new(ClaimTypes.Role, role)
		};

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
			claims: claims,
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomBytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(randomBytes)
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');
	}

	public string HashToken(string token)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(token);
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
		return Convert.ToHexString(hash).ToLowerInvariant();
	}

	public DateTime GetRefreshTokenExpirationUtc()
	{
		var expirationDays = configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7);
		return DateTime.UtcNow.AddDays(expirationDays);
	}

	public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

	public bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}