namespace GerenciadorDePedidos.Core.Entities;

public class Produto : BaseEntity<int>
{
	public string Nome { get; private set; }
	public decimal Preco { get; private set; } 
	public int Estoque { get; private set; }
	public ICollection<MovimentacaoEstoque> MovimentacaoEstoque { get; private set; } = new List<MovimentacaoEstoque>();

	public Produto()
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
		Estoque -= quantidade;
	}

	public void AdicionarEstoque(int quantidade)
	{
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