using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorDePedidos.Infrastructure.Auth;

public class AuthService(IConfiguration configuration) : IAuthService
{
	public string GenerateJwtToken(string email, string role)
	{
		var issuer = configuration["Jwt:Issuer"];
		var audience = configuration["Jwt:Audience"];
		var key = configuration["Jwt:Key"];
		if (string.IsNullOrWhiteSpace(key))
			throw new InvalidOperationException("Jwt:Key não configurada.");

		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new("userName", email),
			new(ClaimTypes.Role, role)
		};

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			expires: DateTime.UtcNow.AddHours(8),
			claims: claims,
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

	public bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}