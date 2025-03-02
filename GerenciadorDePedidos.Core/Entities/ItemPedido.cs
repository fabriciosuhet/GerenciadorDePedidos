namespace GerenciadorDePedidos.Core.Entities;

public class ItemPedido : BaseEntity
{
	public Guid ProdutoId { get; private set; }
	public Produto Produto { get; private set; }
	public int Quantidade { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	public decimal Total { get; private set; }
	
	protected ItemPedido(){}
	
	public ItemPedido(Guid produtoId, int quantidade, decimal precoUnitario)
	{
		ProdutoId = produtoId;
		Quantidade = quantidade;
		PrecoUnitario = precoUnitario;
		Total = quantidade * precoUnitario;
	}

	public void AtualizarTotal()
	{
		Total = PrecoUnitario * Quantidade;
	}
}