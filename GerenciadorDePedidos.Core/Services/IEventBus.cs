namespace GerenciadorDePedidos.Core.Services;

public interface IEventBus
{
	Task PublishAsync<TEvent>(TEvent message, CancellationToken cancellationToken = default) where TEvent : class;
}
