using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Guid>
{
	private readonly IClienteRepository _clienteRepository;

	public CreateClienteCommandHandler(IClienteRepository clienteRepository)
	{
		_clienteRepository = clienteRepository;
	}

	public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = new Cliente(request.NomeCompleto, request.Email, request.Telefone);
		await _clienteRepository.AddAsync(cliente);
		return cliente.Id;
	}
}