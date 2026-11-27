using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public sealed class AdicionarProdutoEstoqueCommandHandler(
	IRepository<Produto, int> produtoRepository,
	IRepository<MovimentacaoEstoque, int> movimentacaoEstoqueRepository,
	IAlertaEstoqueRepository alertaEstoqueRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<AdicionarProdutoEstoqueCommand, int>
{
	public async Task<int> Handle(AdicionarProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.ProdutoId, cancellationToken)
			?? throw new KeyNotFoundException($"O ID: {request.ProdutoId} de produto nao foi encontrado");

		produto.AdicionarEstoque(request.Quantidade);

		var movimentacao = new MovimentacaoEstoque(
			request.Quantidade,
			Tipo.Adicao,
			produto.Id,
			request.ClienteId,
			produto.Preco,
			MotivoMovimentacao.AjusteManual);

		await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);
		produtoRepository.Update(produto);

		if (!produto.EstaComEstoqueBaixo())
		{
			var alertaPendente = await alertaEstoqueRepository.GetPendenteByProdutoIdAsync(produto.Id, cancellationToken);
			if (alertaPendente is not null)
			{
				alertaPendente.Resolver();
				alertaEstoqueRepository.Update(alertaPendente);
			}
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return produto.Id;
	}
}