using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public class RemoverProdutoEstoqueCommandHandler : IRequestHandler<RemoverProdutoEstoqueCommand, int>
{
	private readonly IRepository<Produto ,int> _produtoRepository;
	private readonly IRepository<MovimentacaoEstoque, int> _movimentacaoEstoqueRepository;

    public RemoverProdutoEstoqueCommandHandler(IRepository<Produto, int> produtoRepository, IRepository<MovimentacaoEstoque, int> movimentacaoEstoqueRepository)
    {
        _produtoRepository = produtoRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
    }

    public async Task<int> Handle(RemoverProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await _produtoRepository.GetByIdAsync(request.ProdutoId);
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
		_produtoRepository.UpdateAsync(produto);
		await _movimentacaoEstoqueRepository.SaveChangesAsync();
		
		return produto.Id;
	}
}