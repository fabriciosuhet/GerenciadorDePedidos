namespace GerenciadorDePedidos.Application.Models;

public sealed record ProdutoViewModel(int Id, string Nome, decimal Preco, int Estoque);