namespace GerenciadorDePedidos.Application.Models;

public class ProdutoViewModel
{
	public Guid Id { get;  set; }
	public string Nome { get;  set; }
	public decimal Preco { get;  set; } 
	public int Estoque { get;  set; }

	public ProdutoViewModel(Guid id, string nome, decimal preco, int estoque)
	{
		Id = id;
		Nome = nome;
		Preco = preco;
		Estoque = estoque;
	}
}