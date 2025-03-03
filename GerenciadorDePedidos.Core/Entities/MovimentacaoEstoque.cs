using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class MovimentacaoEstoque : BaseEntity
{
	public int Quantidade { get; private set; }
	public Tipo TipoMovimentacao { get; private set; }
	public DateTime DataMovimentacao { get; private set; } = DateTime.UtcNow;
	public Produto Produto{ get; private set; }
	public Guid ProdutoId { get; private set; }

	public MovimentacaoEstoque(int quantidade, Tipo tipoMovimentacao, Guid produtoId)
	{
		Quantidade = quantidade;
		TipoMovimentacao = tipoMovimentacao;
		ProdutoId = produtoId;

	}
}