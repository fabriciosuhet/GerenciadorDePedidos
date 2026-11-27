using GerenciadorDePedidos.Application.Commands.RegistrarAlertaEstoque;
using GerenciadorDePedidos.Core.Events;
using MassTransit;
using MediatR;

namespace GerenciadorDePedidos.API.Consumers;

public sealed class EstoqueBaixoConsumer(
	ISender sender,
	ILogger<EstoqueBaixoConsumer> logger) : IConsumer<EstoqueBaixoEvent>
{
	public async Task Consume(ConsumeContext<EstoqueBaixoEvent> context)
	{
		var evento = context.Message;
		logger.LogWarning(
			"Alerta de estoque baixo detectado para o produto {ProdutoId} ({ProdutoNome}): estoque atual = {EstoqueAtual}, mínimo = {EstoqueMinimo}, pedido = {PedidoId}.",
			evento.ProdutoId,
			evento.ProdutoNome,
			evento.EstoqueAtual,
			evento.EstoqueMinimo,
			evento.PedidoId);

		await sender.Send(
			new RegistrarAlertaEstoqueCommand(
				evento.ProdutoId,
				evento.ProdutoNome,
				evento.EstoqueAtual,
				evento.EstoqueMinimo,
				evento.PedidoId),
			context.CancellationToken);
	}
}

