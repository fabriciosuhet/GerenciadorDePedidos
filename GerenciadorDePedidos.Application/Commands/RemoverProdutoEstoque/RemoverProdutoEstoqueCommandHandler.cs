using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommandHandler : IRequestHandler<RemoverProdutoEstoqueCommand, Guid>
{
	private readonly IProdutoRepository _repository;
	private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;

	public RemoverProdutoEstoqueCommandHandler(IProdutoRepository repository, IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository)
	{
		_repository = repository;
		_movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
	}

	public async Task<Guid> Handle(RemoverProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await _repository.GetByIdAsync(request.ProdutoId);
		if (produto is null)
			throw new KeyNotFoundException($"O ID: {request.ProdutoId} do produto nao foi encontrado");
		
		if (request.Quantidade <= 0)
		{
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");
		}

		if (request.Quantidade > produto.Estoque)
			throw new ArgumentException(
				$"Não é possível remover {request.Quantidade} do estoque, pois excede o disponível: {produto.Estoque}");
		
		produto.RemoverEstoque(request.Quantidade);

		var movimentacaoEstoque = new MovimentacaoEstoque(request.Quantidade, Tipo.Remocao, request.ProdutoId, request.ClienteId);
		
		await _movimentacaoEstoqueRepository.AddAsync(movimentacaoEstoque);
		await _repository.UpdateAsync(produto.Id, produto);
		
		return produto.Id;
	}
}