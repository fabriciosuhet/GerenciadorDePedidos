using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommand : IRequest<Guid>
{
	public Guid ProdutoId { get; set; }
	public int Quantidade { get; set; }
	public Guid ClienteId { get; set; }

	public RemoverProdutoEstoqueCommand(Guid produtoId, int quantidade, Guid clienteId)
	{
		ProdutoId = produtoId;
		Quantidade = quantidade;
		ClienteId = clienteId;
	}
}