using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, Unit>
{
	private readonly IRepository<Cliente, Guid> _clienteRepository;

    public UpdateClienteCommandHandler(IRepository<Cliente, Guid> clienteRepository)
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
		
		_clienteRepository.UpdateAsync(cliente);
		return Unit.Value;
	}
}