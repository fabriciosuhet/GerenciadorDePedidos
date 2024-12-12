using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public class CreateProdutoCommand : IRequest<Guid>
{
	public string Nome { get; set; }
	public decimal Preco { get; set; } 
	public int Estoque { get; set; }
}