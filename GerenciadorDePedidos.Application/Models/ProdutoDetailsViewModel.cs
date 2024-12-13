namespace GerenciadorDePedidos.Application.Models;

public class ProdutoDetailsViewModel
{
	public string Nome { get; private set; }
	public decimal Preco { get; private set; } 
	public int Estoque { get; private set; }

	public ProdutoDetailsViewModel(string nome, decimal preco, int estoque)
	{
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}
}