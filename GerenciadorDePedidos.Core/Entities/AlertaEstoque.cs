namespace GerenciadorDePedidos.Core.Entities;

public class AlertaEstoque : BaseEntity<int>
{
	public int ProdutoId { get; private set; }
	public Produto Produto { get; private set; } = null!;
	public string ProdutoNome { get; private set; } = string.Empty;
	public int EstoqueAtual { get; private set; }
	public int EstoqueMinimo { get; private set; }
	public int? PedidoId { get; private set; }
	public bool Resolvido { get; private set; }
	public DateTime? ResolvidoEm { get; private set; }

	protected AlertaEstoque()
	{
	}

	public AlertaEstoque(
		int produtoId,
		string produtoNome,
		int estoqueAtual,
		int estoqueMinimo,
		int? pedidoId = null)
	{
		ProdutoId = produtoId;
		ProdutoNome = produtoNome;
		EstoqueAtual = estoqueAtual;
		EstoqueMinimo = estoqueMinimo;
		PedidoId = pedidoId;
		Resolvido = false;
	}

	public void AtualizarNivel(string produtoNome, int estoqueAtual, int estoqueMinimo, int? pedidoId = null)
	{
		ProdutoNome = produtoNome;
		EstoqueAtual = estoqueAtual;
		EstoqueMinimo = estoqueMinimo;
		if (pedidoId.HasValue)
			PedidoId = pedidoId;

		MarcarComoAtualizado();
	}

	public void Resolver(DateTime? dataUtc = null)
	{
		if (Resolvido)
			return;

		var agora = dataUtc ?? DateTime.UtcNow;
		Resolvido = true;
		ResolvidoEm = agora;
		MarcarComoAtualizado(agora);
	}
}

