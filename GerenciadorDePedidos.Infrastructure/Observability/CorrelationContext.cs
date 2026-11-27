using GerenciadorDePedidos.Core.Services;

namespace GerenciadorDePedidos.Infrastructure.Observability;

public sealed class CorrelationContext : ICorrelationContext
{
	private static readonly AsyncLocal<string?> CurrentId = new();

	public string? CorrelationId => CurrentId.Value;

	public IDisposable BeginScope(string correlationId)
	{
		var previous = CurrentId.Value;
		CurrentId.Value = correlationId;
		return new RestoreScope(previous);
	}

	private sealed class RestoreScope(string? previous) : IDisposable
	{
		public void Dispose() => CurrentId.Value = previous;
	}
}

