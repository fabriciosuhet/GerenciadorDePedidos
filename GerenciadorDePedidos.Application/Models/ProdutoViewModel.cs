namespace GerenciadorDePedidos.Application.Models;

public class ProdutoViewModel
{
	public Guid Id { get; private set; }
	public string Nome { get; private set; }
	public decimal Preco { get; private set; } 
	public int Estoque { get; private set; }

	public ProdutoViewModel(Guid id, string nome, decimal preco, int estoque)
	{
		Id = id;
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}
}