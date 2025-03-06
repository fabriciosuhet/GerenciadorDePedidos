using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public class GetAllClientesQueryHandler : IRequestHandler<GetAllClientesQuery, PagedResultModel<ClienteViewModel>>
{
	private readonly IClienteRepository _clienteRepository;

	public GetAllClientesQueryHandler(IClienteRepository clienteRepository)
		=> _clienteRepository = clienteRepository;


	public async Task<PagedResultModel<ClienteViewModel>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
	{
		var count = await _clienteRepository.GetCountAsync(request.Query);
		
		if (count.Equals(0))
			throw new ArgumentException("Nenhum cliente encontrado");

		var clientes = await _clienteRepository.GetPagedAsync(request.Query,
			(request.PageNumber - 1) * request.PageSize, request.PageSize);
		
		var clienteViewModel = clientes
			.Select(c => new ClienteViewModel(c.Id, c.NomeCompleto, c.Email, c.Telefone))
			.ToList();
		
		return new PagedResultModel<ClienteViewModel>(clienteViewModel, count, request.PageNumber, request.PageSize);
	}
}