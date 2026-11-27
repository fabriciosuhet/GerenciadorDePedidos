using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAlertasEstoque;

public sealed class GetAlertasEstoqueQueryHandler(IAlertaEstoqueRepository alertaEstoqueRepository)
	: IRequestHandler<GetAlertasEstoqueQuery, List<AlertaEstoqueViewModel>>
{
	public async Task<List<AlertaEstoqueViewModel>> Handle(
		GetAlertasEstoqueQuery request,
		CancellationToken cancellationToken)
	{
		var alertas = await alertaEstoqueRepository.GetAllAsync(request.ApenasPendentes, cancellationToken);

		return alertas.Select(a => new AlertaEstoqueViewModel(
			a.Id,
			a.ProdutoId,
			a.ProdutoNome,
			a.EstoqueAtual,
			a.EstoqueMinimo,
			a.PedidoId,
			a.Resolvido,
			a.CriadoEm,
			a.ResolvidoEm)).ToList();
	}
}

