using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public class AdicionarProdutoEstoqueCommandHandler : IRequestHandler<AdicionarProdutoEstoqueCommand, Guid>
{
	private readonly IProdutoRepository _repository;
	private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;

	public AdicionarProdutoEstoqueCommandHandler(IProdutoRepository repository, IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository)
	{
		_repository = repository;
		_movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
	}
	
	public async Task<Guid> Handle(AdicionarProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		
		var produto = await _repository.GetByIdAsync(request.ProdutoId);
		if (produto is null)
			throw new KeyNotFoundException($"O ID: {request.ProdutoId} de produto nao foi encontrado");
		
		if (request.Quantidade <= 0)
		{
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");
		}
		
		produto.AdicionarEstoque(request.Quantidade);

		var movimentacaoEstoque = new MovimentacaoEstoque(request.Quantidade, Tipo.Adicao, produto.Id, request.ClienteId);
		
		await _movimentacaoEstoqueRepository.AddAsync(movimentacaoEstoque);
		await _repository.UpdateAsync(produto.Id, produto);
		
		return produto.Id;
	}
}