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

	[Fact]
	public void TransicoesDeStatus_DevemSeguirCicloValido()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente);

		pedido.MarcarComoProcessado();
		Assert.Equal(Status.Processado, pedido.Status);

		pedido.Concluir();
		Assert.Equal(Status.Concluido, pedido.Status);
		Assert.NotNull(pedido.ConcluidoEm);

		Assert.Throws<InvalidOperationException>(() => pedido.Cancelar());
		Assert.Throws<InvalidOperationException>(() => pedido.CancelarComEstorno());

		pedido.Devolver("Produto com avaria na embalagem");
		Assert.Equal(Status.Devolvido, pedido.Status);
		Assert.NotNull(pedido.DevolvidoEm);
		Assert.Equal("Produto com avaria na embalagem", pedido.MotivoDevolucao);

		Assert.Throws<InvalidOperationException>(() => pedido.Cancelar());
		Assert.Throws<InvalidOperationException>(() => pedido.CancelarComEstorno());
		Assert.Throws<InvalidOperationException>(() => pedido.Devolver("Segunda tentativa"));
	}

	[Fact]
	public void CancelarComEstorno_DeveRetornarTrueParaProcessadoFalseParaPendenteEBloquearConcluido()
	{
		var pedidoPendente = new Pedido(Guid.NewGuid(), Status.Pendente);
		Assert.False(pedidoPendente.CancelarComEstorno());
		Assert.Equal(Status.Cancelado, pedidoPendente.Status);
		Assert.Throws<InvalidOperationException>(() => pedidoPendente.CancelarComEstorno());

		var pedidoProcessado = new Pedido(Guid.NewGuid(), Status.Pendente);
		pedidoProcessado.MarcarComoProcessado();
		Assert.True(pedidoProcessado.CancelarComEstorno());
		Assert.Equal(Status.Cancelado, pedidoProcessado.Status);

		var pedidoConcluido = new Pedido(Guid.NewGuid(), Status.Pendente);
		pedidoConcluido.MarcarComoProcessado();
		pedidoConcluido.Concluir();
		Assert.Throws<InvalidOperationException>(() => pedidoConcluido.CancelarComEstorno());
	}

	[Fact]
	public void Devolver_ForaDeConcluidoOuSemMotivo_DeveLancarExcecao()
	{
		var pedidoPendente = new Pedido(Guid.NewGuid(), Status.Pendente);
		Assert.Throws<InvalidOperationException>(() => pedidoPendente.Devolver("Desistência"));

		var pedidoProcessado = new Pedido(Guid.NewGuid(), Status.Pendente);
		pedidoProcessado.MarcarComoProcessado();
		Assert.Throws<InvalidOperationException>(() => pedidoProcessado.Devolver("Desistência"));

		var pedidoConcluido = new Pedido(Guid.NewGuid(), Status.Pendente);
		pedidoConcluido.MarcarComoProcessado();
		pedidoConcluido.Concluir();
		Assert.Throws<ArgumentException>(() => pedidoConcluido.Devolver("   "));
	}

	[Fact]
	public void BaseEntity_DesativarEReativar_DeveAtualizarAuditoria()
	{
		var produto = new Produto("Monitor", 1000m, 5);
		Assert.True(produto.Ativo);
		Assert.Null(produto.DesativadoEm);

		produto.Desativar();
		Assert.False(produto.Ativo);
		Assert.NotNull(produto.DesativadoEm);
		Assert.NotNull(produto.AtualizadoEm);

		produto.Reativar();
		Assert.True(produto.Ativo);
		Assert.Null(produto.DesativadoEm);
	}
}