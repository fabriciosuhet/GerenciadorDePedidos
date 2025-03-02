using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public class AdicionarProdutoEstoqueCommandHandler : IRequestHandler<AdicionarProdutoEstoqueCommand, Guid>
{
	private readonly IProdutoRepository _repository;

	public AdicionarProdutoEstoqueCommandHandler(IProdutoRepository repository)
	{
		_repository = repository;
	}

	public async Task<Guid> Handle(AdicionarProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Adicionando {request.Quantidade} ao produto {request.ProdutoId}");
		
		var produto = await _repository.GetByIdAsync(request.ProdutoId);
		if (produto is null)
			throw new KeyNotFoundException($"O ID: {request.ProdutoId} de produto nao foi encontrado");
		
		produto.AdicionarEstoque(request.Quantidade);
		await _repository.UpdateAsync(produto.Id, produto);
		
		return produto.Id;
	}
}