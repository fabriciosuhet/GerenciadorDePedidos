using MediatR;

namespace GerenciadorDePedidos.Application.Commands;

public class UpdateProdutoCommand : IRequest<Unit>
{
	public int Id { get; set; }
	public string Nome { get; set; }
	public decimal Preco { get; set; }
	public int Estoque { get; set; }
}