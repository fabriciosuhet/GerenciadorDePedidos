using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ProdutoTests
{
	[Fact]
	public void Construtor_DeveCriarProdutoComDadosCorretos()
	{
		const string nome = "produto 1";
		const decimal preco = 10m;
		const int estoque = 100;

		var produto = new Produto(nome, preco, estoque);

		Assert.Equal(nome, produto.Nome);
		Assert.Equal(preco, produto.Preco);
		Assert.Equal(estoque, produto.Estoque);
	}

	[Fact]
	public void AdicionarEstoque_DeveAdicionarCorretamente()
	{
		var produto = new Produto("produto 1", 15m, 5);

		produto.AdicionarEstoque(3);

		Assert.Equal(8, produto.Estoque);
	}

	[Fact]
	public void AdicionarEstoque_DeveLancarExcecaoSeQuantidadeInvalida()
	{
		var produto = new Produto("produto 1", 15m, 0);

		var exception = Assert.Throws<ArgumentException>(() => produto.AdicionarEstoque(0));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
	}

	[Fact]
	public void RemoverEstoque_DeveRemoverCorretamente()
	{
		var produto = new Produto("produto 1", 15m, 5);

		produto.RemoverEstoque(2);

		Assert.Equal(3, produto.Estoque);
	}

	[Fact]
	public void RemoverEstoque_QuantidadeInvalidaOuMaiorQueEstoque_DeveLancarExcecao()
	{
		var produto = new Produto("produto 1", 15m, 5);

		var zeroEx = Assert.Throws<ArgumentException>(() => produto.RemoverEstoque(0));
		Assert.Equal("O valor não pode ser menor ou igual a 0", zeroEx.Message);

		var excedeEx = Assert.Throws<ArgumentException>(() => produto.RemoverEstoque(10));
		Assert.Equal("Não é possível remover 10 do estoque, pois excede o disponível: 5", excedeEx.Message);
	}

	[Fact]
	public void AlterarNome_DeveAtualizarNomeCorretamente()
	{
		var produto = new Produto("produto 1", 15m, 0);

		produto.AlterarNome("produto 2");

		Assert.Equal("produto 2", produto.Nome);
	}

	[Fact]
	public void AlterarPreco_DeveAtualizarPrecoCorretamente()
	{
		var produto = new Produto("produto 1", 15m, 0);

		produto.AlterarPreco(30m);

		Assert.Equal(30m, produto.Preco);
	}
}