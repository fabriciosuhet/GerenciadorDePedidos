using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public class GetAllClientesQueryHandler : IRequestHandler<GetAllClientesQuery, PagedResultModel<ClienteViewModel>>
{
	private readonly IRepository<Cliente, Guid> _clienteRepository;

    public GetAllClientesQueryHandler(IRepository<Cliente, Guid> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<PagedResultModel<ClienteViewModel>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
	{
		var count = await _clienteRepository.GetCountAsync();
		
		if (count.Equals(0))
			return new PagedResultModel<ClienteViewModel>([], 0, request.PageNumber, request.PageSize);

		var clientes = await _clienteRepository.GetPagedAsync(
			(request.PageNumber - 1) * request.PageSize, request.PageSize);
		
		var clienteViewModel = clientes
			.Select(c => new ClienteViewModel(c.Id, c.NomeCompleto, c.Email, c.Telefone))
			.ToList();
		
		return new PagedResultModel<ClienteViewModel>(clienteViewModel, count, request.PageNumber, request.PageSize);
	}
}