using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class AdicionarProdutoEstoqueCommandHandlerTests
{
	[Fact]
	public async Task Handle_CommandValid_AddsMovimentacaoEstoqueUpdatesProdutoAndResolvesAlert()
	{
		const int produtoId = 1;
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto Teste", 11.99m, 2, estoqueMinimo: 5) { Id = produtoId };
		var alertaPendente = new AlertaEstoque(produtoId, "Produto Teste", 2, 5);

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var alertaRepositoryMock = new Mock<IAlertaEstoqueRepository>();
		alertaRepositoryMock.Setup(r => r.GetPendenteByProdutoIdAsync(produtoId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(alertaPendente);
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			alertaRepositoryMock.Object,
			unitOfWorkMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(produtoId, result);
		Assert.Equal(12, produto.Estoque);
		Assert.True(alertaPendente.Resolvido);
		Assert.NotNull(alertaPendente.ResolvidoEm);
		produtoRepositoryMock.Verify(r => r.Update(produto), Times.Once);
		alertaRepositoryMock.Verify(r => r.Update(alertaPendente), Times.Once);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(
			It.Is<MovimentacaoEstoque>(m =>
				m.Quantidade == 10 &&
				m.TipoMovimentacao == Tipo.Adicao &&
				m.ProdutoId == produtoId &&
				m.ClienteId == clienteId &&
				m.PrecoUnitario == 11.99m &&
				m.Motivo == MotivoMovimentacao.AjusteManual &&
				m.PedidoId == null),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_InvalidProdutoId_ThrowsKeyNotFoundException()
	{
		const int produtoId = 1;
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var alertaRepositoryMock = new Mock<IAlertaEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, Guid.NewGuid());
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			alertaRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O ID: {produtoId} de produto nao foi encontrado", exception.Message);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_NegativeQuantidade_ThrowsArgumentException()
	{
		const int produtoId = 1;
		var produto = new Produto("Produto Teste", 10.99m, 10) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var alertaRepositoryMock = new Mock<IAlertaEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, -5, Guid.NewGuid());
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			alertaRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
		produtoRepositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
	}
}