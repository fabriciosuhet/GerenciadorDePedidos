namespace GerenciadorDePedidos.Core.Events;

public sealed record EstoqueBaixoEvent(
	int ProdutoId,
	string ProdutoNome,
	int EstoqueAtual,
	int EstoqueMinimo,
	DateTime DetectadoEmUtc,
	int? PedidoId = null);

