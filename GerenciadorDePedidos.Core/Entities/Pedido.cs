using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Pedido : BaseEntity<int>
{
	public DateTime DataPedido { get; private set; } = DateTime.UtcNow;
	public List<ItemPedido> ItensPedidos { get; private set; } = [];
	public Status Status { get; private set; }
	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; } = null!;

	public decimal Total => ItensPedidos.Sum(item => item.Total);

	protected Pedido()
	{
	}

	public Pedido(Guid clienteId, Status status = Status.Pendente)
	{
		ClienteId = clienteId;
		Status = status;
		ItensPedidos = [];
	}

	public void AdicionarItem(Produto produto, int quantidade)
	{
		ArgumentNullException.ThrowIfNull(produto);

		var item = new ItemPedido(produto.Id, quantidade, produto.Preco);
		ItensPedidos.Add(item);
	}
}