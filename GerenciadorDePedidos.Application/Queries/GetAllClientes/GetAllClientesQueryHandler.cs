using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public sealed class GetAllClientesQueryHandler(IClienteRepository clienteRepository)
	: IRequestHandler<GetAllClientesQuery, PagedResultModel<ClienteViewModel>>
{
	public async Task<PagedResultModel<ClienteViewModel>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
	{
		var count = await clienteRepository.GetCountAsync(request.Query, cancellationToken);
		if (count == 0)
			return new PagedResultModel<ClienteViewModel>([], 0, request.PageNumber, request.PageSize);

		var clientes = await clienteRepository.GetPagedAsync(
			request.Query,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize,
			cancellationToken);

		var items = clientes
			.Select(c => new ClienteViewModel(c.Id, c.NomeCompleto, c.Telefone))
			.ToList();

		return new PagedResultModel<ClienteViewModel>(items, count, request.PageNumber, request.PageSize);
	}
}