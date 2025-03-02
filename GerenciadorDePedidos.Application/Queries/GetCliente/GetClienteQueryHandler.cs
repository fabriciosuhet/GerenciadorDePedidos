using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
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

		var pedidosDto = cliente.Pedidos.Select(p => new PedidoRespondeDTO
		{
			Id = p.Id,
			DataPedido = p.DataPedido,
			Total = p.Total,
			ClienteId = p.ClienteId,
			ClienteNome = p.Cliente.NomeCompleto,
			ItensPedidos = p.ItensPedidos.Select(ip => new ItemPedidoResponseDTO
			{
				Id = ip.Id,
				ProdutoId = ip.ProdutoId,
				ProdutoNome = ip.Produto.Nome,
				Quantidade = ip.Quantidade,
				PrecoUnitario = ip.PrecoUnitario,
				Total = ip.Total
				
			}).ToList()
		}).ToList();

		return new ClienteDetailsViewModel(
			cliente.NomeCompleto,
			cliente.Email,
			cliente.Telefone,
			pedidosDto
		);
	}
	
}