namespace GerenciadorDePedidos.Core.DTOs;

public class ItemPedidoResponseDTO
{
	public Guid Id { get;  set; }
	public Guid ProdutoId { get; set; }
	public string ProdutoNome { get; set; }
	public int Quantidade { get; set; }
	public decimal PrecoUnitario { get; set; }
	public decimal Total { get;  set; }
}