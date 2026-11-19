using GerenciadorDePedidos.Core.Exceptions;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public sealed class DeleteClienteCommandHandler(IClienteRepository clienteRepository)
	: IRequestHandler<DeleteClienteCommand, Unit>
{
	public async Task<Unit> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = await clienteRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("O cliente nao foi encontrado");

		if (await clienteRepository.HasVinculosAsync(cliente.Id, cancellationToken))
			throw new ConflictException("Não é possível excluir este cliente porque ele possui pedidos ou movimentações de estoque vinculadas.");

		await clienteRepository.DeleteAsync(cliente.Id, cancellationToken);
		await clienteRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}