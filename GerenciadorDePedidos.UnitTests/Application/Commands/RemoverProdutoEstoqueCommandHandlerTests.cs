using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class RemoverProdutoEstoqueCommandHandlerTests
{
	[Fact]
	public async Task Handle_CommandValid_RemoveMovimentacaoProdutoEstoqueAndUpdatesProduto()
	{
		// Arrange
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto 1", 19.99m, 20)
		{
			Id = produtoId
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		produtoRepositoryMock.Setup(r => r.UpdateAsync(produtoId, It.IsAny<Produto>())).Returns(Task.CompletedTask);
		
		// Mock de IMovimentacaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		movimentacaoEstoqueRepositoryMock.Setup(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()))
			.Returns(Task.CompletedTask);

		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new RemoverProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.Equal(produto.Id, result);
		Assert.Equal(10, produto.Estoque);
		produtoRepositoryMock.Verify(r => r.GetByIdAsync(produtoId), Times.Once());
		produtoRepositoryMock.Verify(r => r.UpdateAsync(produtoId, It.Is<Produto>(p => p == produto)), Times.Once());
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.Is<MovimentacaoEstoque>(m =>
			m.ProdutoId.Equals(produtoId) && m.Quantidade.Equals(10) && m.TipoMovimentacao.Equals(Tipo.Remocao) && m.ClienteId.Equals(clienteId))), Times.Once());
	}

	[Fact]
	public async Task Handle_InvalidProdutoId_ThrowsKeyNotFoundException()
	{
		// Arrange
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync((Produto)null);
		
		// Mock de IMovimentacaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		
		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new RemoverProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O ID: {produtoId} do produto nao foi encontrado", exception.Message);
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()), Times.Never());
		
	}

	[Fact]
	public async Task Handle_NegativaQuantidade_ThrowsArgumentException()
	{
		// Arrange 
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto teste", 19.99m, 10)
		{
			Id = produtoId
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		
		// Mock de IMovimentecaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		var command = new RemoverProdutoEstoqueCommand(produtoId, -5, clienteId);
		var handler = new RemoverProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
		produtoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Produto>()), Times.Never());
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()), Times.Never());

	}

	[Fact]
	public async Task Handle_QuantidadeExceedsEstoque_ThrowsArgumentException()
	{
		// Arrange
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto teste", 19.99m, 5)
		{
			Id = produtoId
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		
		// Mock de IMovimentacaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new RemoverProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"Não é possível remover 10 do estoque, pois excede o disponível: 5", exception.Message);
		produtoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Produto>()), Times.Never());
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()), Times.Never());
	}
	
}