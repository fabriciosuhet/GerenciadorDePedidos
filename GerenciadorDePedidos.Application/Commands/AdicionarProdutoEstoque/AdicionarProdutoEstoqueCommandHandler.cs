using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public class AdicionarProdutoEstoqueCommandHandler : IRequestHandler<AdicionarProdutoEstoqueCommand, int>
{
	private readonly IRepository<Produto, int> _produtoRepository;
	private readonly IRepository<MovimentacaoEstoque, int> _movimentacaoEstoqueRepository;

    public AdicionarProdutoEstoqueCommandHandler(IRepository<Produto, int> produtoRepository, IRepository<MovimentacaoEstoque, int> movimentacaoEstoqueRepository)
    {
        _produtoRepository = produtoRepository;
        _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
    }

    public async Task<int> Handle(AdicionarProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		
		var produto = await _produtoRepository.GetByIdAsync(request.ProdutoId);
		if (produto is null)
			throw new KeyNotFoundException($"O ID: {request.ProdutoId} de produto nao foi encontrado");
		
		if (request.Quantidade <= 0)
		{
			throw new ArgumentException("O valor não pode ser menor ou igual a 0");
		}
		
		produto.AdicionarEstoque(request.Quantidade);

		var movimentacaoEstoque = new MovimentacaoEstoque(request.Quantidade, Tipo.Adicao, produto.Id, request.ClienteId);
		
		await _movimentacaoEstoqueRepository.AddAsync(movimentacaoEstoque);
        _produtoRepository.UpdateAsync(produto);
		await _produtoRepository.SaveChangesAsync();
        await _movimentacaoEstoqueRepository.SaveChangesAsync();


        return produto.Id;
	}
}