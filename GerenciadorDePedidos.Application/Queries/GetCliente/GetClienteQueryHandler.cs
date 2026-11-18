using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetCliente;

public sealed class GetClienteQueryHandler(IClienteRepository clienteRepository)
	: IRequestHandler<GetClienteQuery, ClienteDetailsViewModel?>
{
	public async Task<ClienteDetailsViewModel?> Handle(GetClienteQuery request, CancellationToken cancellationToken)
	{
		var cliente = await clienteRepository.GetDetailsByIdAsync(request.Id, cancellationToken);
		if (cliente is null)
			return null;

		var pedidosDto = cliente.Pedidos.Select(p => p.ToResponseDto()).ToList();

		return new ClienteDetailsViewModel(cliente.NomeCompleto, cliente.Telefone, pedidosDto);
	}
}