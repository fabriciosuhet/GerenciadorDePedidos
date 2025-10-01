using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public class GetPedidoQueryHandler : IRequestHandler<GetPedidoQuery, PedidoRespondeDTO>
{
	private readonly IRepository<Pedido, int> _pedidoRepository;

    public GetPedidoQueryHandler(IRepository<Pedido, int> pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<PedidoRespondeDTO> Handle(GetPedidoQuery request, CancellationToken cancellationToken)
	{
		var pedido = await _pedidoRepository.GetByIdAsync(request.Id);
		if (pedido is null) return null;

		return new PedidoRespondeDTO
		{
			Id = pedido.Id,
			DataPedido = TimeZoneInfo.ConvertTimeFromUtc(pedido.DataPedido,
				TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")),
			Total = pedido.Total,
			ClienteId = pedido.ClienteId,
			ClienteNome = pedido.Cliente.NomeCompleto,
			Status = pedido.Status,
			ItensPedidos = pedido.ItensPedidos.Select(i => new ItemPedidoResponseDTO
			{
				Id = i.Id,
				ProdutoId = i.ProdutoId,
				ProdutoNome = i.Produto.Nome,
				Quantidade = i.Quantidade,
				PrecoUnitario = i.PrecoUnitario,
				Total = i.Total,

			}).ToList()
		};

	}
}