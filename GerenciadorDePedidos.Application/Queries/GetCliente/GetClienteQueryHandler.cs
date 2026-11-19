using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetCliente;

public sealed class GetClienteQueryHandler(
	IClienteRepository clienteRepository,
	ICurrentUserService currentUserService)
	: IRequestHandler<GetClienteQuery, ClienteDetailsViewModel?>
{
	public async Task<ClienteDetailsViewModel?> Handle(GetClienteQuery request, CancellationToken cancellationToken)
	{
		if (!currentUserService.IsAdmin && currentUserService.ClienteId != request.Id)
			throw new UnauthorizedAccessException("Você não tem permissão para acessar os dados de outro cliente.");

		var cliente = await clienteRepository.GetDetailsByIdAsync(request.Id, cancellationToken);
		if (cliente is null)
			return null;

		var pedidosDto = cliente.Pedidos
			.OrderByDescending(p => p.DataPedido)
			.ThenByDescending(p => p.Id)
			.Select(p => p.ToResponseDto())
			.ToList();

		return new ClienteDetailsViewModel(cliente.NomeCompleto, cliente.Telefone, pedidosDto);
	}
}