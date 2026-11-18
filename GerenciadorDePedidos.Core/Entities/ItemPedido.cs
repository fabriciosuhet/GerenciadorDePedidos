namespace GerenciadorDePedidos.Core.Entities;

public class ItemPedido : BaseEntity<int>
{
	public int ProdutoId { get; private set; }
	public Produto Produto { get; private set; } = null!;
	public int Quantidade { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	public decimal Total { get; private set; }

	protected ItemPedido()
	{
	}

	public ItemPedido(int produtoId, int quantidade, decimal precoUnitario)
	{
		if (quantidade <= 0)
			throw new ArgumentException("A quantidade deve ser maior que zero.");

		if (precoUnitario < 0)
			throw new ArgumentException("O preço unitário não pode ser negativo.");

		ProdutoId = produtoId;
		Quantidade = quantidade;
		PrecoUnitario = precoUnitario;
		AtualizarTotal();
	}

	public void AtualizarTotal() => Total = PrecoUnitario * Quantidade;
}