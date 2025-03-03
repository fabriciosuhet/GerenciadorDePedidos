using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class PedidoTests
{
	[Fact]
	public void Construtor_DeveCriarPedidosComDadosCorretos()
	{
		// Arrange (preparar)
		var itensPedidos = new List<ItemPedido>();
		var total = 0m;
		var clienteId = Guid.NewGuid();
		var dataPedido = DateTime.UtcNow;
		
		// Act (Agir)
		var pedido = new Pedido(itensPedidos, total, clienteId, dataPedido, Status.Pendente);
		
		// Assert (Verificar)
		Assert.IsEmpty(pedido.ItensPedidos);
		Assert.AreEqual(total, pedido.Total);
		Assert.AreEqual(clienteId, pedido.ClienteId);
		Assert.That(pedido.DataPedido, Is.EqualTo(dataPedido).Within(TimeSpan.FromSeconds(1)));

	}

	[Fact]
	public void ConstrutorPadrao_DeveInicilizarListaDePedidosNaoNula()
	{
		// Arrange e Act
		var pedido = new Pedido(new List<ItemPedido>(), 0m, Guid.NewGuid(), DateTime.UtcNow, Status.Pendente);
		
		// Assert
		Assert.NotNull(pedido.ItensPedidos);
	}

	[Fact]
	public void TotalPedidos_DeveAtualizarTotalCorretamente()
	{
		// Arrange
		var pedido = new Pedido(new List<ItemPedido>(), 0m, Guid.NewGuid(), DateTime.UtcNow, Status.Pendente);
		var quantidade = 10;
		var precoUnitario = 50m;
		var totalEsperado = quantidade * precoUnitario;
		
		// Act
		pedido.TotalPedido(quantidade, precoUnitario);
		
		// Assert
		Assert.AreEqual(totalEsperado, pedido.Total);
		
		
		
	}
}