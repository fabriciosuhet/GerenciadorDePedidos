using GerenciadorDePedidos.Core.Entities;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ProdutoTestes
{
	[Fact]
	public void Construtor_DeveCriarProdutoComDadosCorretos()
	{
		// Arrange
		const string nome = "produto 1";
		const decimal preco = 0m;
		const int estoque = 100;
		
		// Act
		var produto = new Produto(nome, preco, estoque);
		
		// Assert
		Assert.AreEqual(nome, produto.Nome);
		Assert.AreEqual(preco, produto.Preco);
		Assert.AreEqual(estoque, produto.Estoque);
		
	}

	[Fact]
	public void AdicionarEstoque_DeveAdicionarCorretamente()
	{
		// Arrange
		var produto = new Produto("produto 1", 15m, 0);
		var quantidade = 1;
		var estoque = 0;
		var totalEsperado = produto.Estoque + quantidade;
		
		// Act
		produto.AdicionarEstoque(quantidade);

		// Assert
		Assert.AreEqual(totalEsperado, produto.Estoque);

	}

	[Fact]
	public void AdicionarEstoque_DeveLancarExcecaoSeQuantidadeInvalida()
	{
		// Arrange
		var produto = new Produto("produto 1", 15m, 0);
		
		// Act e Assert
		var exception = Assert.Throws<ArgumentException>(() => produto.AdicionarEstoque(0));
		Assert.AreEqual("O valor não pode ser menor ou igual a 0" , exception.Message);
	}

	[Fact]
	public void RemoverEstoque_DeveRemoverCorretamente()
	{
		// Arrange 
		var produto = new Produto("produto 1", 15m, 1);
		var quantidade = 1;
		var estoque = 1;
		var totalEsperado = produto.Estoque - quantidade;
		
		// Act
		produto.RemoverEstoque(quantidade);
		
		// Assert
		var exception = Assert.Throws<ArgumentException>(() => produto.RemoverEstoque(0));
		Assert.AreEqual("O valor não pode ser menor ou igual a 0", exception.Message);
	}

	[Fact]
	public void Nome_AlterarNomeCorretamente()
	{
		// Arrange
		var produto = new Produto("produto 1", 15m, 0);
		var nome = "produto 2";
		
		// Act
		produto.AlterarNome(nome);
		
		// Assert
		Assert.AreEqual(nome, produto.Nome);
	}

	[Fact]
	public void Preco_AlterarPrecoCorretamente()
	{
		// Arrange 
		var produto = new Produto("produto 1", 15m, 0);
		var preco = 30m;
		
		// Act
		produto.AlterarPreco(preco);
		
		// Assert
		Assert.AreEqual(preco, produto.Preco);
	}
}