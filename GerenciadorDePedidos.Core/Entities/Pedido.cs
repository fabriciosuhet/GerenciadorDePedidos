namespace GerenciadorDePedidos.Core.Entities;

public class Pedido : BaseEntity
{
	public DateTime DataPedido { get; private set; } = DateTime.Now;
	public List<ItemPedido> ItensPedidos { get; private set; }
	public decimal Total { get; private set; }

	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; }
	
	public Pedido()
	{
		ItensPedidos = new List<ItemPedido>();
		Total = 0;
	}
	
	
}