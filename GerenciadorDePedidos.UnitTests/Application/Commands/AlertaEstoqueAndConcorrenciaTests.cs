using GerenciadorDePedidos.Application.Commands.RegistrarAlertaEstoque;
using GerenciadorDePedidos.Application.Commands.ResolverAlertaEstoque;
using GerenciadorDePedidos.Application.Queries.GetAlertasEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class AlertaEstoqueAndConcorrenciaTests
{
	[Fact]
	public void Produto_MutacoesDeEstado_DevemIncrementarVersaoEAvaliarEstoqueBaixo()
	{
		var produto = new Produto("SSD 1TB", 450m, 10, estoqueMinimo: 4);
		Assert.Equal(1, produto.Versao);
		Assert.False(produto.EstaComEstoqueBaixo());

		produto.RemoverEstoque(6); // estoque = 4 <= 4
		Assert.Equal(2, produto.Versao);
		Assert.True(produto.EstaComEstoqueBaixo());

		produto.AdicionarEstoque(5); // estoque = 9 > 4
		Assert.Equal(3, produto.Versao);
		Assert.False(produto.EstaComEstoqueBaixo());

		produto.AlterarEstoqueMinimo(10); // estoque = 9 <= 10
		Assert.Equal(4, produto.Versao);
		Assert.True(produto.EstaComEstoqueBaixo());
	}

	[Fact]
	public async Task RegistrarAlerta_QuandoNaoHaPendente_DeveCriarNovoAlerta()
	{
		var repoMock = new Mock<IAlertaEstoqueRepository>();
		repoMock.Setup(r => r.GetPendenteByProdutoIdAsync(10, It.IsAny<CancellationToken>()))
			.ReturnsAsync((AlertaEstoque?)null);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RegistrarAlertaEstoqueCommandHandler(repoMock.Object, unitOfWorkMock.Object);

		await handler.Handle(new RegistrarAlertaEstoqueCommand(10, "SSD 1TB", 2, 5, 77), CancellationToken.None);

		repoMock.Verify(r => r.AddAsync(
			It.Is<AlertaEstoque>(a =>
				a.ProdutoId == 10 &&
				a.ProdutoNome == "SSD 1TB" &&
				a.EstoqueAtual == 2 &&
				a.EstoqueMinimo == 5 &&
				a.PedidoId == 77 &&
				!a.Resolvido),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RegistrarAlerta_QuandoJaExistePendente_DeveAtualizarSemDuplicar()
	{
		var existente = new AlertaEstoque(10, "SSD 1TB", 4, 5) { Id = 3 };
		var repoMock = new Mock<IAlertaEstoqueRepository>();
		repoMock.Setup(r => r.GetPendenteByProdutoIdAsync(10, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existente);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RegistrarAlertaEstoqueCommandHandler(repoMock.Object, unitOfWorkMock.Object);

		var id = await handler.Handle(new RegistrarAlertaEstoqueCommand(10, "SSD 1TB", 1, 5, 88), CancellationToken.None);

		Assert.Equal(3, id);
		Assert.Equal(1, existente.EstoqueAtual);
		Assert.Equal(88, existente.PedidoId);
		repoMock.Verify(r => r.AddAsync(It.IsAny<AlertaEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
		repoMock.Verify(r => r.Update(existente), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ResolverAlerta_QuandoExiste_DeveMarcarComoResolvido()
	{
		var alerta = new AlertaEstoque(10, "SSD 1TB", 2, 5) { Id = 9 };
		var repoMock = new Mock<IAlertaEstoqueRepository>();
		repoMock.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(alerta);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new ResolverAlertaEstoqueCommandHandler(repoMock.Object, unitOfWorkMock.Object);

		await handler.Handle(new ResolverAlertaEstoqueCommand(9), CancellationToken.None);

		Assert.True(alerta.Resolvido);
		Assert.NotNull(alerta.ResolvidoEm);
		repoMock.Verify(r => r.Update(alerta), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ResolverAlerta_QuandoNaoExiste_DeveLancarKeyNotFoundException()
	{
		var repoMock = new Mock<IAlertaEstoqueRepository>();
		repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((AlertaEstoque?)null);

		var handler = new ResolverAlertaEstoqueCommandHandler(repoMock.Object, new Mock<IUnitOfWork>().Object);

		await Assert.ThrowsAsync<KeyNotFoundException>(() =>
			handler.Handle(new ResolverAlertaEstoqueCommand(999), CancellationToken.None));
	}

	[Fact]
	public async Task GetAlertasEstoque_DeveMapearViewModels()
	{
		var alertas = new List<AlertaEstoque>
		{
			new(1, "Mouse", 2, 5, 10) { Id = 1 }
		};
		var repoMock = new Mock<IAlertaEstoqueRepository>();
		repoMock.Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(alertas);

		var handler = new GetAlertasEstoqueQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAlertasEstoqueQuery(true), CancellationToken.None);

		Assert.Single(result);
		Assert.Equal("Mouse", result[0].ProdutoNome);
		Assert.Equal(2, result[0].EstoqueAtual);
		Assert.False(result[0].Resolvido);
	}
}

