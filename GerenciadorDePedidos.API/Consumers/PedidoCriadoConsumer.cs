using GerenciadorDePedidos.Application.Commands.ProcessarPedido;
using GerenciadorDePedidos.Core.Events;
using MassTransit;
using MediatR;

namespace GerenciadorDePedidos.API.Consumers;

public sealed class PedidoCriadoConsumer(
	ISender sender,
	ILogger<PedidoCriadoConsumer> logger) : IConsumer<PedidoCriadoEvent>
{
	public async Task Consume(ConsumeContext<PedidoCriadoEvent> context)
	{
		var pedidoId = context.Message.PedidoId;
		logger.LogInformation("Processando pedido {PedidoId} recebido da fila.", pedidoId);

		var processado = await sender.Send(new ProcessarPedidoCommand(pedidoId), context.CancellationToken);

		if (processado)
			logger.LogInformation("Pedido {PedidoId} processado com sucesso, estoque reservado e aguardando entrega.", pedidoId);
		else
			logger.LogWarning("Pedido {PedidoId} não processado (cancelado por falta de estoque ou já processado).", pedidoId);
	}
}
