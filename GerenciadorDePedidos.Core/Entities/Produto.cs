namespace GerenciadorDePedidos.Core.Entities;

public class Produto : BaseEntity<int>
{
	public const int EstoqueMinimoPadrao = 5;

	public string Nome { get; private set; } = string.Empty;
	public decimal Preco { get; private set; }
	public int Estoque { get; private set; }
	public int EstoqueMinimo { get; private set; } = EstoqueMinimoPadrao;
	public int Versao { get; private set; } = 1;
	public ICollection<MovimentacaoEstoque> MovimentacaoEstoque { get; private set; } = new List<MovimentacaoEstoque>();

	protected Produto()
	{
	}

	public Produto(string nome, decimal preco, int estoque, int estoqueMinimo = EstoqueMinimoPadrao)
	{
		if (estoqueMinimo < 0)
			throw new ArgumentException("O estoque mínimo não pode ser negativo.");

		Nome = nome;
		Preco = preco;
		Estoque = estoque;
		EstoqueMinimo = estoqueMinimo;
	}

	public bool EstaComEstoqueBaixo() => Estoque <= EstoqueMinimo;

	public void RemoverEstoque(int quantidade)
	{
		if (quantidade <= 0)
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");

		if (quantidade > Estoque)
			throw new ArgumentException(
				$"Não é possível remover {quantidade} do estoque, pois excede o disponível: {Estoque}");

		Estoque -= quantidade;
		IncrementarVersao();
	}

	public void AdicionarEstoque(int quantidade)
	{
		if (quantidade <= 0)
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");

		Estoque += quantidade;
		IncrementarVersao();
	}

	public void AlterarDados(string nome, decimal preco, int estoque, int? estoqueMinimo = null)
	{
		AlterarNome(nome);
		AlterarPreco(preco);
		Estoque = estoque;

		if (estoqueMinimo.HasValue)
			AlterarEstoqueMinimo(estoqueMinimo.Value);
		else
			IncrementarVersao();
	}

	public void AlterarNome(string nome)
	{
		if (string.IsNullOrWhiteSpace(nome))
			throw new ArgumentException("O nome do produto não pode ser vazio.");

		Nome = nome;
		IncrementarVersao();
	}

	public void AlterarPreco(decimal preco)
	{
		if (preco < 0)
			throw new ArgumentException("O preço não pode ser negativo.");

		Preco = preco;
		IncrementarVersao();
	}

	public void AlterarEstoqueMinimo(int estoqueMinimo)
	{
		if (estoqueMinimo < 0)
			throw new ArgumentException("O estoque mínimo não pode ser negativo.");

		EstoqueMinimo = estoqueMinimo;
		IncrementarVersao();
	}

	private void IncrementarVersao() => Versao++;
}