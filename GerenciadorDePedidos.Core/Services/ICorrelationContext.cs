namespace GerenciadorDePedidos.Core.Services;

public interface ICorrelationContext
{
	string? CorrelationId { get; }
	IDisposable BeginScope(string correlationId);
}

