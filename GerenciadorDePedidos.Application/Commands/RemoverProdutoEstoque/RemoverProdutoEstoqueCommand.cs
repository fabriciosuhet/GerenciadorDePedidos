using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommand : IRequest<Guid>
{
	public Guid ProdutoId { get; set; }
	public int Quantidade { get; set; }

	public RemoverProdutoEstoqueCommand(Guid produtoId, int quantidade)
	{
		ProdutoId = produtoId;
		Quantidade = quantidade;
	}
}