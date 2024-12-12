namespace GerenciadorDePedidos.Core.Entities;

public class ItemPedido : BaseEntity
{
	public Guid ProdutoId { get; private set; }
	public Produto Produto { get; private set; }
	public int Quantidade { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	
	protected ItemPedido(){}

	public ItemPedido(Guid produtoId, Produto produto, int quantidade, decimal precoUnitario)
	{
		ProdutoId = produtoId;
		Produto = produto;
		Quantidade = quantidade;
		PrecoUnitario = precoUnitario;
	}
}