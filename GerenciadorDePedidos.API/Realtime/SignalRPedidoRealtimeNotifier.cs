using GerenciadorDePedidos.API.Hubs;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Services;
using Microsoft.AspNetCore.SignalR;

namespace GerenciadorDePedidos.API.Realtime;

public sealed class SignalRPedidoRealtimeNotifier(
	IHubContext<PedidosHub> hubContext,
	ILogger<SignalRPedidoRealtimeNotifier> logger) : IPedidoRealtimeNotifier
{
	public async Task NotificarStatusAtualizadoAsync(
		PedidoStatusAtualizadoDTO notificacao,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// Clients.Groups deduplica conexões presentes em ambos os grupos (ex.: Admin visualizando próprio pedido).
			IReadOnlyList<string> grupos = [PedidosHub.AdminsGroup, PedidosHub.ClienteGroup(notificacao.ClienteId)];

			await hubContext.Clients
				.Groups(grupos)
				.SendAsync(PedidosHub.EventoPedidoStatusAtualizado, notificacao, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogWarning(
				ex,
				"Falha ao enviar notificação SignalR para PedidoId {PedidoId} (Status={Status}).",
				notificacao.PedidoId,
				notificacao.StatusAtual);
		}
	}
}

