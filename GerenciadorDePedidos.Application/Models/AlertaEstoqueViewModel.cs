namespace GerenciadorDePedidos.Application.Models;

public sealed record AlertaEstoqueViewModel(
	int Id,
	int ProdutoId,
	string ProdutoNome,
	int EstoqueAtual,
	int EstoqueMinimo,
	int? PedidoId,
	bool Resolvido,
	DateTime CriadoEm,
	DateTime? ResolvidoEm);

