using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class MovimentacaoEstoque : BaseEntity<int>
{
	public int Quantidade { get; private set; }
	public Tipo TipoMovimentacao { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	public MotivoMovimentacao Motivo { get; private set; } = MotivoMovimentacao.AjusteManual;
	public DateTime DataMovimentacao { get; private set; } = DateTime.UtcNow;
	public Produto Produto { get; private set; } = null!;
	public int ProdutoId { get; private set; }

	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; } = null!;

	public int? PedidoId { get; private set; }
	public Pedido? Pedido { get; private set; }

	public decimal ValorTotal => Quantidade * PrecoUnitario;

	protected MovimentacaoEstoque()
	{
	}

	public MovimentacaoEstoque(
		int quantidade,
		Tipo tipoMovimentacao,
		int produtoId,
		Guid clienteId,
		decimal precoUnitario = 0m,
		MotivoMovimentacao motivo = MotivoMovimentacao.AjusteManual,
		int? pedidoId = null)
	{
		if (precoUnitario < 0)
			throw new ArgumentException("O preço unitário da movimentação não pode ser negativo.");

		Quantidade = quantidade;
		TipoMovimentacao = tipoMovimentacao;
		ProdutoId = produtoId;
		ClienteId = clienteId;
		PrecoUnitario = precoUnitario;
		Motivo = motivo;
		PedidoId = pedidoId;
	}
}