using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RegistrarAlertaEstoque;

public sealed class RegistrarAlertaEstoqueCommandHandler(
	IAlertaEstoqueRepository alertaEstoqueRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<RegistrarAlertaEstoqueCommand, int>
{
	public async Task<int> Handle(RegistrarAlertaEstoqueCommand request, CancellationToken cancellationToken)
	{
		var alertaExistente = await alertaEstoqueRepository.GetPendenteByProdutoIdAsync(
			request.ProdutoId,
			cancellationToken);

		if (alertaExistente is not null)
		{
			alertaExistente.AtualizarNivel(
				request.ProdutoNome,
				request.EstoqueAtual,
				request.EstoqueMinimo,
				request.PedidoId);
			alertaEstoqueRepository.Update(alertaExistente);
			await unitOfWork.SaveChangesAsync(cancellationToken);
			return alertaExistente.Id;
		}

		var novoAlerta = new AlertaEstoque(
			request.ProdutoId,
			request.ProdutoNome,
			request.EstoqueAtual,
			request.EstoqueMinimo,
			request.PedidoId);

		await alertaEstoqueRepository.AddAsync(novoAlerta, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return novoAlerta.Id;
	}
}

