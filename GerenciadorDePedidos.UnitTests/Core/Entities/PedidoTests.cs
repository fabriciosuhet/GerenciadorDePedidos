using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class PedidoTests
{
	[Fact]
	public void Construtor_DeveCriarPedidosComDadosCorretos()
	{
		var clienteId = Guid.NewGuid();
		var antes = DateTime.UtcNow;

		var pedido = new Pedido(clienteId, Status.Pendente);

		Assert.Empty(pedido.ItensPedidos);
		Assert.Equal(0m, pedido.Total);
		Assert.Equal(clienteId, pedido.ClienteId);
		Assert.Equal(Status.Pendente, pedido.Status);
		Assert.InRange(pedido.DataPedido, antes.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
	}

	[Fact]
	public void AdicionarItem_DeveAtualizarTotalCorretamente()
	{
		var clienteId = Guid.NewGuid();
		var pedido = new Pedido(clienteId, Status.Pendente);
		var produto = new Produto("Teclado", 50m, 20) { Id = 1 };

		pedido.AdicionarItem(produto, 10);

		Assert.Single(pedido.ItensPedidos);
		Assert.Equal(500m, pedido.Total);
	}
}