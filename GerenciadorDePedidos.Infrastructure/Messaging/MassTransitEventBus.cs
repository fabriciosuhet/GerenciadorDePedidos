using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Services;
using MassTransit;

namespace GerenciadorDePedidos.Infrastructure.Messaging;

public sealed class MassTransitEventBus(
	IPublishEndpoint publishEndpoint,
	ICorrelationContext correlationContext) : IEventBus
{
	public Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
		where TEvent : class
	{
		var correlationId = correlationContext.CorrelationId;

		if (string.IsNullOrWhiteSpace(correlationId))
			return publishEndpoint.Publish(message, cancellationToken);

		return publishEndpoint.Publish(message, context =>
		{
			context.Headers.Set(AppDiagnostics.CorrelationIdHeader, correlationId);
			if (Guid.TryParse(correlationId, out var correlationGuid))
				context.CorrelationId = correlationGuid;
		}, cancellationToken);
	}
}
