using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public class GetAllClientesQueryHandler : IRequestHandler<GetAllClientesQuery, List<ClienteViewModel>>
{
	private readonly IClienteRepository _clienteRepository;

	public GetAllClientesQueryHandler(IClienteRepository clienteRepository)
		=> _clienteRepository = clienteRepository;


	public async Task<List<ClienteViewModel>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
	{
		var cliente = await _clienteRepository.GetAllAsync(request.Query);
		var clienteViewModel = cliente
			.Select(c => new ClienteViewModel(c.Id, c.NomeCompleto, c.Email, c.Telefone))
			.ToList();
		return clienteViewModel;
	}
}