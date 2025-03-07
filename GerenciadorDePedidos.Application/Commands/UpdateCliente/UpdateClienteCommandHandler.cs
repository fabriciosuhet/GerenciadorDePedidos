using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, Unit>
{
	private readonly IClienteRepository _clienteRepository;

	public UpdateClienteCommandHandler(IClienteRepository clienteRepository)
	{
		_clienteRepository = clienteRepository;
	}

	public async Task<Unit> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente  = await _clienteRepository.GetByIdAsync(request.Id);
		if (cliente is null)
			throw new KeyNotFoundException("Cliente nao encontrado");
		
		cliente.AlterarEmail(request.Email);
		cliente.AlterarTelefone(request.Telefone);
		
		await _clienteRepository.UpdateAsync(cliente.Id, cliente);
		return Unit.Value;
	}
}