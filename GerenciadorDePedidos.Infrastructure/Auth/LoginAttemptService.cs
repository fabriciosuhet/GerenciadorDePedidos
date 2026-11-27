using System.Security.Cryptography;
using System.Text;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GerenciadorDePedidos.Infrastructure.Auth;

public sealed class LoginAttemptService(
	ICacheService cacheService,
	IConfiguration configuration,
	ILogger<LoginAttemptService> logger) : ILoginAttemptService
{
	private int MaxFailedAttempts => Math.Max(1, configuration.GetValue("AuthLockout:MaxFailedAttempts", 5));
	private TimeSpan LockoutDuration => TimeSpan.FromMinutes(Math.Max(1, configuration.GetValue("AuthLockout:LockoutMinutes", 15)));
	private bool IsEnabled => configuration.GetValue("AuthLockout:Enabled", true);

	public bool IsLockedOut(string email)
	{
		if (!IsEnabled || string.IsNullOrWhiteSpace(email))
			return false;

		var key = BuildKey(email);
		return cacheService.TryGet<LoginAttemptState>(key, out var state)
			&& state is not null
			&& state.LockedUntilUtc.HasValue
			&& state.LockedUntilUtc.Value > DateTime.UtcNow;
	}

	public void RegisterFailedAttempt(string email)
	{
		if (!IsEnabled || string.IsNullOrWhiteSpace(email))
			return;

		var key = BuildKey(email);
		var agora = DateTime.UtcNow;
		var maxAttempts = MaxFailedAttempts;
		var duration = LockoutDuration;

		cacheService.TryGet<LoginAttemptState>(key, out var current);
		var failedCount = (current?.FailedAttempts ?? 0) + 1;

		DateTime? lockedUntil = current?.LockedUntilUtc is { } existingLock && existingLock > agora
			? existingLock
			: failedCount >= maxAttempts
				? agora.Add(duration)
				: null;

		if (lockedUntil.HasValue && current?.LockedUntilUtc is null)
		{
			logger.LogWarning(
				"Conta temporariamente bloqueada após {Tentativas} tentativas inválidas de login. Bloqueio até {LockedUntilUtc:O}.",
				failedCount,
				lockedUntil.Value);
		}

		cacheService.Set(key, new LoginAttemptState(failedCount, lockedUntil), duration);
	}

	public void ResetAttempts(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			return;

		cacheService.Remove(BuildKey(email));
	}

	private static string BuildKey(string email)
	{
		var normalized = email.Trim().ToLowerInvariant();
		var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
		return $"auth:lockout:{hash}";
	}

	private sealed record LoginAttemptState(int FailedAttempts, DateTime? LockedUntilUtc);
}

