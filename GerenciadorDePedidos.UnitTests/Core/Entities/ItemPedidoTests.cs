using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ItemPedidoTests
{
	[Fact]
	public void Construtor_DeveCriarItemPedidoComDadosCorretos()
	{
		const int produtoId = 1;
		const int quantidade = 2;
		const decimal precoUnitario = 10m;
		var totalEsperado = quantidade * precoUnitario;

		var itemPedido = new ItemPedido(produtoId, quantidade, precoUnitario);

		Assert.Equal(produtoId, itemPedido.ProdutoId);
		Assert.Equal(quantidade, itemPedido.Quantidade);
		Assert.Equal(precoUnitario, itemPedido.PrecoUnitario);
		Assert.Equal(totalEsperado, itemPedido.Total);
	}

	[Fact]
	public void Construtor_QuantidadeInvalida_DeveLancarExcecao()
	{
		Assert.Throws<ArgumentException>(() => new ItemPedido(1, 0, 10m));
	}

	[Fact]
	public void AtualizarTotal_DeveManterTotalConsistente()
	{
		var itemPedido = new ItemPedido(1, 4, 10m);
		var totalEsperado = itemPedido.Quantidade * itemPedido.PrecoUnitario;

		itemPedido.AtualizarTotal();

		Assert.Equal(totalEsperado, itemPedido.Total);
	}
}