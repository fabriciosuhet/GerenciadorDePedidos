using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetCliente;

public class GetClienteQueryHandler : IRequestHandler<GetClienteQuery, ClienteDetailsViewModel>
{
	private readonly IClienteRepository _clienteRepository;

	public GetClienteQueryHandler(IClienteRepository clienteRepository)
	{
		_clienteRepository = clienteRepository;
	}

	public async Task<ClienteDetailsViewModel> Handle(GetClienteQuery request, CancellationToken cancellationToken)
	{
		var cliente = await _clienteRepository.GetDetailsByIdAsync(request.Id);
		if (cliente == null) return null;

		var clienteDetailsViewModel = new ClienteDetailsViewModel(
			cliente.NomeCompleto,
			cliente.Email,
			cliente.Telefone,
			cliente.Pedidos
		);
		return clienteDetailsViewModel;
	}
}