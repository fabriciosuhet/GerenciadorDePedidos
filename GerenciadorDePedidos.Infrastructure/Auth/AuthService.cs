using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorDePedidos.Infrastructure.Auth;

public class AuthService : IAuthService
{
	private readonly IConfiguration _configuration;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
	{
		_configuration = configuration;
		_httpContextAccessor = httpContextAccessor;
	}

	public string GenerateJwtToken(string email, string role)
	{
		var issuer = _configuration["Jwt:Issuer"];
		var audience = _configuration["Jwt:Audience"];
		var key = _configuration["Jwt:Key"];
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new Claim("userName", email),
			new Claim(ClaimTypes.Role, role)
		};

		var token = new JwtSecurityToken(issuer: issuer, audience: audience, expires: DateTime.Now.AddHours(8),
			claims: claims, signingCredentials: credentials);

		var tokenHandler = new JwtSecurityTokenHandler();
		var stringToken = tokenHandler.WriteToken(token);
		
		return stringToken;
	}

	public string ComputeSha256Hash(string password)
	{
		using (SHA256 sha256Hashg = SHA256.Create())
		{
			// Compute hash - retorna byte array
			byte[] bytes = sha256Hashg.ComputeHash(Encoding.UTF8.GetBytes(password));
			
			// Converte byte array em string
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}

	public bool IsAuthenticated()
	{
		return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
	}

	public bool IsInRole(Role role)
	{
		return _httpContextAccessor.HttpContext?.User.IsInRole(role.ToString()) ?? false;
	}
}