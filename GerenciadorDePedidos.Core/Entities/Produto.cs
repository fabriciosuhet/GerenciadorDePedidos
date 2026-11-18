namespace GerenciadorDePedidos.Core.Entities;

public class Produto : BaseEntity<int>
{
	public string Nome { get; private set; } = string.Empty;
	public decimal Preco { get; private set; }
	public int Estoque { get; private set; }
	public ICollection<MovimentacaoEstoque> MovimentacaoEstoque { get; private set; } = new List<MovimentacaoEstoque>();

	protected Produto()
	{
	}

	public Produto(string nome, decimal preco, int estoque)
	{
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}

	public void RemoverEstoque(int quantidade)
	{
		if (quantidade <= 0)
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");

		if (quantidade > Estoque)
			throw new ArgumentException(
				$"Não é possível remover {quantidade} do estoque, pois excede o disponível: {Estoque}");

		Estoque -= quantidade;
	}

	public void AdicionarEstoque(int quantidade)
	{
		if (quantidade <= 0)
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");

		Estoque += quantidade;
	}

	public void AlterarDados(string nome, decimal preco, int estoque)
	{
		AlterarNome(nome);
		AlterarPreco(preco);
		Estoque = estoque;
	}

	public void AlterarNome(string nome)
	{
		if (string.IsNullOrWhiteSpace(nome))
			throw new ArgumentException("O nome do produto não pode ser vazio.");

		Nome = nome;
	}

	public void AlterarPreco(decimal preco)
	{
		if (preco < 0)
			throw new ArgumentException("O preço não pode ser negativo.");

		Preco = preco;
	}
}