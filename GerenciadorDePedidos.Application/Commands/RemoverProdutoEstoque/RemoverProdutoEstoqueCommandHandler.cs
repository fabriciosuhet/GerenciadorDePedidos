using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Events;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public sealed class RemoverProdutoEstoqueCommandHandler(
	IRepository<Produto, int> produtoRepository,
	IRepository<MovimentacaoEstoque, int> movimentacaoEstoqueRepository,
	IUnitOfWork unitOfWork,
	IEventBus eventBus) : IRequestHandler<RemoverProdutoEstoqueCommand, int>
{
	public async Task<int> Handle(RemoverProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.ProdutoId, cancellationToken)
			?? throw new KeyNotFoundException($"O ID: {request.ProdutoId} do produto nao foi encontrado");

		produto.RemoverEstoque(request.Quantidade);

		var movimentacao = new MovimentacaoEstoque(
			request.Quantidade,
			Tipo.Remocao,
			produto.Id,
			request.ClienteId,
			produto.Preco,
			MotivoMovimentacao.AjusteManual);

		await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);
		produtoRepository.Update(produto);

		if (produto.EstaComEstoqueBaixo())
		{
			await eventBus.PublishAsync(
				new EstoqueBaixoEvent(
					produto.Id,
					produto.Nome,
					produto.Estoque,
					produto.EstoqueMinimo,
					DateTime.UtcNow),
				cancellationToken);
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return produto.Id;
	}
}