using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Pedido : BaseEntity<int>
{
	public DateTime DataPedido { get; private set; } = DateTime.UtcNow;
	public List<ItemPedido> ItensPedidos { get; private set; } = [];
	public Status Status { get; private set; }
	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; } = null!;
	public DateTime? ConcluidoEm { get; private set; }
	public DateTime? DevolvidoEm { get; private set; }
	public string? MotivoDevolucao { get; private set; }

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
		MarcarComoAtualizado();
	}

	public void Concluir()
	{
		if (Status != Status.Processado)
			throw new InvalidOperationException($"Não é possível concluir um pedido com status {Status}. Apenas pedidos processados podem ser concluídos.");

		var agora = DateTime.UtcNow;
		Status = Status.Concluido;
		ConcluidoEm = agora;
		MarcarComoAtualizado(agora);
	}

	public void Cancelar()
	{
		if (Status == Status.Concluido)
			throw new InvalidOperationException("Não é possível cancelar um pedido já concluído/entregue. Utilize o fluxo de devolução.");

		if (Status == Status.Devolvido)
			throw new InvalidOperationException("Não é possível cancelar um pedido já devolvido.");

		if (Status == Status.Cancelado)
			throw new InvalidOperationException("Este pedido já está cancelado.");

		Status = Status.Cancelado;
		MarcarComoAtualizado();
	}

	/// <summary>
	/// Cancela um pedido ainda não entregue (Pendente ou Processado) e indica se o estoque precisa ser estornado.
	/// </summary>
	public bool CancelarComEstorno()
	{
		if (Status == Status.Concluido)
			throw new InvalidOperationException("Não é possível cancelar um pedido já concluído/entregue. Utilize o fluxo de devolução.");

		if (Status == Status.Devolvido)
			throw new InvalidOperationException("Não é possível cancelar um pedido já devolvido.");

		if (Status == Status.Cancelado)
			throw new InvalidOperationException("Este pedido já está cancelado.");

		var requerEstorno = Status == Status.Processado;
		Status = Status.Cancelado;
		MarcarComoAtualizado();
		return requerEstorno;
	}

	/// <summary>
	/// Registra a devolução de um pedido já concluído/entregue.
	/// </summary>
	public void Devolver(string motivo)
	{
		if (Status == Status.Devolvido)
			throw new InvalidOperationException("Este pedido já foi devolvido.");

		if (Status != Status.Concluido)
			throw new InvalidOperationException($"Não é possível devolver um pedido com status {Status}. Apenas pedidos concluídos/entregues podem ser devolvidos.");

		if (string.IsNullOrWhiteSpace(motivo))
			throw new ArgumentException("O motivo da devolução é obrigatório.");

		var agora = DateTime.UtcNow;
		Status = Status.Devolvido;
		DevolvidoEm = agora;
		MotivoDevolucao = motivo.Trim();
		MarcarComoAtualizado(agora);
	}
}