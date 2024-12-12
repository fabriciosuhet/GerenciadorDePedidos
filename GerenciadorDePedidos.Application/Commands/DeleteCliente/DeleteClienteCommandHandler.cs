using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, Unit>
{
	private readonly IClienteRepository _clienteRepository;

	public DeleteClienteCommandHandler(IClienteRepository clienteRepository)
	{
		_clienteRepository = clienteRepository;
	}

	public async Task<Unit> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = await _clienteRepository.GetByIdAsync(request.Id);
		await _clienteRepository.DeleteAsync(cliente.Id);
		return Unit.Value;
	}
}