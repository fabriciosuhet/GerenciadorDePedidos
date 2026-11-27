using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ResolverAlertaEstoque;

public sealed class ResolverAlertaEstoqueCommandHandler(
	IAlertaEstoqueRepository alertaEstoqueRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<ResolverAlertaEstoqueCommand, Unit>
{
	public async Task<Unit> Handle(ResolverAlertaEstoqueCommand request, CancellationToken cancellationToken)
	{
		var alerta = await alertaEstoqueRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Alerta de estoque nao encontrado");

		alerta.Resolver();
		alertaEstoqueRepository.Update(alerta);
		await unitOfWork.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}

