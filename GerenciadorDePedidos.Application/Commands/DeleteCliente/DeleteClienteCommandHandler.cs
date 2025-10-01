using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, Unit>
{
	private readonly IRepository<Cliente, Guid> _clienteRepository;

    public DeleteClienteCommandHandler(IRepository<Cliente, Guid> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Unit> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = await _clienteRepository.GetByIdAsync(request.Id);
		if (cliente is null)
			throw new KeyNotFoundException("O cliente nao foi encontrado");

		await _clienteRepository.DeleteAsync(cliente.Id);
		return Unit.Value;
	}
}