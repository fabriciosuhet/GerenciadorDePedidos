namespace GerenciadorDePedidos.Core.DTOs;

public class ItemPedidoResponseDTO
{
	public int Id { get;  set; }
	public int ProdutoId { get; set; }
	public string ProdutoNome { get; set; }
	public int Quantidade { get; set; }
	public decimal PrecoUnitario { get; set; }
	public decimal Total { get;  set; }
}