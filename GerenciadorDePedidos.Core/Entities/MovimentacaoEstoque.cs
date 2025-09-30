using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class MovimentacaoEstoque : BaseEntity<int>
{
	public int Quantidade { get; private set; }
	public Tipo TipoMovimentacao { get; private set; }
	public DateTime DataMovimentacao { get; private set; } = DateTime.UtcNow;
	public Produto Produto{ get; private set; }
	public int ProdutoId { get; private set; }

	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; }
	
	public MovimentacaoEstoque(){}

	public MovimentacaoEstoque(int quantidade, Tipo tipoMovimentacao, int produtoId, Guid clienteId)
	{
		Quantidade = quantidade;
		TipoMovimentacao = tipoMovimentacao;
		ProdutoId = produtoId;
		ClienteId = clienteId;
	}
}