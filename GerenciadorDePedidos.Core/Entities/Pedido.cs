using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Pedido : BaseEntity<int>
{
	public DateTime DataPedido { get; set; } = DateTime.UtcNow;
	public List<ItemPedido> ItensPedidos { get; private set; }
	public Status Status { get; private set; }
	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; }

	public decimal Total => ItensPedidos.Sum(item => item.Total);
	
	public Pedido(){}
	
	public Pedido(Guid clienteId, Status status)
	{
		ClienteId = clienteId;
		Status = Status.Pendente;
        ItensPedidos = [];
    }
	
	public void AdicionarItem(int produtoId, int quantidade, decimal precoUnitario)
	{
		var item = new ItemPedido(produtoId, quantidade, precoUnitario);
		ItensPedidos.Add(item);
    }
}