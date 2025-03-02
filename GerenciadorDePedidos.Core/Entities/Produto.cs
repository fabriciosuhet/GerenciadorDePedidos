namespace GerenciadorDePedidos.Core.Entities;

public class Produto : BaseEntity
{
	public string Nome { get; private set; }
	public decimal Preco { get; private set; } 
	public int Estoque { get; private set; }

	public Produto(string nome, decimal preco, int estoque)
	{
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}

	public void RemoverEstoque(int quantidade)
	{
		if (quantidade <= 0)
		{
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");
		}
		Estoque -= quantidade;
	}

	public void AdicionarEstoque(int quantidade)
	{
		if (quantidade <= 0)
		{
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");
		}
		Estoque += quantidade;
	}

	public void AlterarDados(string nome, decimal preco, int estoque)
	{
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}
	
	public void AlterarNome(string nome)
	{
		Nome = nome;
	}

	public void AlterarPreco(decimal preco)
	{
		Preco = preco;
	}
	
}