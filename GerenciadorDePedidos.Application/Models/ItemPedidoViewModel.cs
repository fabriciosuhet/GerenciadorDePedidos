using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ItemPedidoViewModel
{
	public Guid Id { get; private set; }
	public string ProdutoNome { get; private set; }
	public int Quantidade { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	public decimal Total => Quantidade * PrecoUnitario;

	public ItemPedidoViewModel(Guid id, string produtoNome, int quantidade, decimal precoUnitario)
	{
		Id = id;
		ProdutoNome = produtoNome;
		Quantidade = quantidade;
		PrecoUnitario = precoUnitario;
	}
}