using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public sealed class DeleteClienteCommandHandler(IRepository<Cliente, Guid> clienteRepository)
	: IRequestHandler<DeleteClienteCommand, Unit>
{
	public async Task<Unit> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = await clienteRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("O cliente nao foi encontrado");

		await clienteRepository.DeleteAsync(cliente.Id, cancellationToken);
		await clienteRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}