namespace GerenciadorDePedidos.Application.Models;

public class ProdutoViewModel
{
	public string Nome { get; private set; }
	public decimal Preco { get; private set; } 
	public int Estoque { get; private set; }

	public ProdutoViewModel(string nome, decimal preco, int estoque)
	{
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}
}