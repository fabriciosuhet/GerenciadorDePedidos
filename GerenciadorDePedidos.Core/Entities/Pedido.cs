namespace GerenciadorDePedidos.Core.Entities;

public class Pedido : BaseEntity
{
	public DateTime DataPedido { get; private set; } = DateTime.Now;
	public List<ItemPedido> ItensPedidos { get; private set; }
	public decimal Total { get; private set; }

	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; }
	
	protected Pedido(){}
	
	public Pedido(List<ItemPedido> itensPedidos, decimal total, Guid clienteId)
	{
		ItensPedidos = itensPedidos;
		Total = total;
		ClienteId = clienteId;
	}
	
	public void TotalPedido(int quantidade, decimal precoUnitario)
	{
		Total += quantidade * precoUnitario;
	}
}