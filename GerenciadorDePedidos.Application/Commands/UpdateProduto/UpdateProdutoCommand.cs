using MediatR;

namespace GerenciadorDePedidos.Application.Commands;

public class UpdateProdutoCommand : IRequest<Unit>
{
	public Guid Id { get; set; }
	public string Nome { get; set; }
	public decimal Preco { get; set; } 
}