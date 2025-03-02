using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommandHandler : IRequestHandler<RemoverProdutoEstoqueCommand, Guid>
{
	private readonly IProdutoRepository _repository;
	
	public RemoverProdutoEstoqueCommandHandler(IProdutoRepository repository)
	{
		_repository = repository;
	}

	public async Task<Guid> Handle(RemoverProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await _repository.GetByIdAsync(request.ProdutoId);
		if (produto is null)
			throw new KeyNotFoundException($"O ID: {request.ProdutoId} do produto nao foi encontrado");

		if (request.Quantidade > produto.Estoque)
			throw new ArgumentException(
				$"Nao é possivel remover a quantidade: {request.Quantidade}  maior do que no estoque: {produto.Estoque}.");
		
		produto.RemoverEstoque(request.Quantidade);
		await _repository.UpdateAsync(produto.Id, produto);
		
		return produto.Id;
	}
}