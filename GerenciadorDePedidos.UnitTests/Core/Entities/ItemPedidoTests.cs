using GerenciadorDePedidos.Core.Entities;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ItemPedidoTests
{
	[Fact]
	public void Construtor_DeveCriarItemPedidoComDadosCorretos()
	{
		// Arrange
		var produtoId = 1;
		const int quantidade = 2;
		const decimal precoUnitario = 10m;
		var totalEsperado = quantidade * precoUnitario;
		
		// Act
		var itemPedido = new ItemPedido(produtoId, quantidade, precoUnitario);
		
		// Arrange
		Assert.AreEqual(produtoId, itemPedido.ProdutoId);
		Assert.AreEqual(quantidade, itemPedido.Quantidade);
		Assert.AreEqual(precoUnitario, itemPedido.PrecoUnitario);
		Assert.AreEqual(totalEsperado, itemPedido.Total);
	}

	[Fact]
	public void Total_DeveSerCalculadoCorretamenteNoConstrutor()
	{
		// Arrange
		var produtoId = 1;
		var quantidade = 3;
		var precoUnitario = 5m;
		var totalEsperado = quantidade * precoUnitario;
		
		// Act
		var itemPedido = new ItemPedido(produtoId, quantidade, precoUnitario);
		
		// Assert
		Assert.AreEqual(totalEsperado, itemPedido.Total);
		
	}

	[Fact]
	public void AtualizarTotal_DeveManterTotalConsistente()
	{
		// Arrange
		var itemPedido = new ItemPedido(1, 4, 10m);
		var totalEsperado = itemPedido.Quantidade * itemPedido.PrecoUnitario;
		
		// Act
		itemPedido.AtualizarTotal();
		
		// Assert
		Assert.AreEqual(totalEsperado, itemPedido.Total);
	}
}