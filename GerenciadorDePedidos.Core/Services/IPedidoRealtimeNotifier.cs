using GerenciadorDePedidos.Core.DTOs;

namespace GerenciadorDePedidos.Core.Services;

public interface IPedidoRealtimeNotifier
{
	Task NotificarStatusAtualizadoAsync(
		PedidoStatusAtualizadoDTO notificacao,
		CancellationToken cancellationToken = default);
}

