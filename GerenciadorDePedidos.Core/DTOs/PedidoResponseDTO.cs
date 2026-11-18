using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public class PedidoResponseDTO
{
	public int Id { get; set; }
	public DateTime DataPedido { get; set; }
	public decimal Total { get; set; }
	public Guid ClienteId { get; set; }
	public string ClienteNome { get; set; } = string.Empty;
	public Status Status { get; set; }
	public List<ItemPedidoResponseDTO> ItensPedidos { get; set; } = [];
}
