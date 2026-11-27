using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RegistrarAlertaEstoque;

public sealed record RegistrarAlertaEstoqueCommand(
	int ProdutoId,
	string ProdutoNome,
	int EstoqueAtual,
	int EstoqueMinimo,
	int? PedidoId = null) : IRequest<int>;

