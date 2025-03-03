using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;

public class AdicionarProdutoEstoqueCommand : IRequest<Guid>
{
	public Guid ProdutoId { get; set; }
	public int Quantidade { get; set; }
	public Guid ClienteId { get; set; }

	public AdicionarProdutoEstoqueCommand(Guid produtoId, int quantidade, Guid clienteId)
	{
		ProdutoId = produtoId;
		Quantidade = quantidade;
		ClienteId = clienteId;
	}
}