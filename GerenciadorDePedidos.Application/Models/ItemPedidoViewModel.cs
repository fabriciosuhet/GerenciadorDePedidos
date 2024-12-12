using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ItemPedidoViewModel
{
	public string ProdutoNome { get; private set; }
	public int Quantidade { get; private set; }
	public decimal PrecoUnitario { get; private set; }
	public decimal Total => Quantidade * PrecoUnitario;

	public ItemPedidoViewModel(string produtoNome, int quantidade, decimal precoUnitario)
	{
		ProdutoNome = produtoNome;
		Quantidade = quantidade;
		PrecoUnitario = precoUnitario;
	}
}