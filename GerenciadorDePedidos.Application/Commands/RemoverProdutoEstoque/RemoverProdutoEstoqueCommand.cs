using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommand : IRequest<int>
{
	public int ProdutoId { get; set; }
	public int Quantidade { get; set; }
	public Guid ClienteId { get; set; }

	public RemoverProdutoEstoqueCommand(int produtoId, int quantidade, Guid clienteId)
	{
		ProdutoId = produtoId;
		Quantidade = quantidade;
		ClienteId = clienteId;
	}
}