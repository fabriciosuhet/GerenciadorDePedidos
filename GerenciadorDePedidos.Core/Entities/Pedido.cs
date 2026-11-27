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

	public void MarcarComoProcessado()
	{
		if (Status != Status.Pendente)
			throw new InvalidOperationException($"Não é possível processar um pedido com status {Status}.");

		Status = Status.Processado;
	}

	public void Concluir()
	{
		if (Status != Status.Processado)
			throw new InvalidOperationException($"Não é possível concluir um pedido com status {Status}.");

		Status = Status.Concluido;
	}

	public void Cancelar()
	{
		if (Status == Status.Concluido)
			throw new InvalidOperationException("Não é possível cancelar um pedido já concluído.");

		if (Status == Status.Cancelado)
			throw new InvalidOperationException("Este pedido já está cancelado.");

		Status = Status.Cancelado;
		MarcarComoAtualizado();
	}

	/// <summary>
	/// Cancela o pedido e indica se o estoque dos itens precisa ser estornado.
	/// </summary>
	public bool CancelarComEstorno()
	{
		if (Status == Status.Cancelado)
			throw new InvalidOperationException("Este pedido já está cancelado.");

		var requerEstorno = Status is Status.Processado or Status.Concluido;
		Status = Status.Cancelado;
		MarcarComoAtualizado();
		return requerEstorno;
	}
}