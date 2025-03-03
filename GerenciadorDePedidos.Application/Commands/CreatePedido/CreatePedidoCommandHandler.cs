using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommandHandler : IRequestHandler<CreatePedidoCommand, Guid>
{
	private readonly IPedidoRepository _pedidoRepository;
	private readonly IProdutoRepository _produtoRepository;

	public CreatePedidoCommandHandler(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository)
	{
		_pedidoRepository = pedidoRepository;
		_produtoRepository = produtoRepository;
	}

	public async Task<Guid> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
	{

		if (request is null || request.ItensPedidos is null || !request.ItensPedidos.Any())
			throw new ArgumentException("O pedido deve conter pelo menos um item.");

		var itens = new List<ItemPedido>();
		foreach (var itemDto in request.ItensPedidos)
		{
			// buscar o produto no repositorio
			var produto = await _produtoRepository.GetByIdAsync(itemDto.produtoId);
			
			if (produto is null)
				throw new ArgumentException($"O produto com o ID {itemDto.produtoId} nao foi encontrado.");
			
			// cria o itemDto usando o preço do produto
			var item = new ItemPedido(itemDto.produtoId, itemDto.Quantidade, produto.Preco);
			itens.Add(item);
		}

		var total = itens.Sum(item => item.Total);
		var pedido = new Pedido(itens, total, request.ClienteId, DateTime.UtcNow, request.Status);
		
		await _pedidoRepository.AddAsync(pedido);
		return pedido.Id;

	}
}