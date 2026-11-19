using GerenciadorDePedidos.Core.Services;
using MassTransit;

namespace GerenciadorDePedidos.Infrastructure.Messaging;

public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
	public Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default)
		where TEvent : class
		=> publishEndpoint.Publish(message, cancellationToken);
}
