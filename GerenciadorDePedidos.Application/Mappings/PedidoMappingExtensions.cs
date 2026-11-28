using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Mappings;

public static class TimeZoneHelper
{
	private static readonly TimeZoneInfo BrasiliaTimeZone = ResolveBrasiliaTimeZone();

	public static DateTime ToBrasiliaTime(this DateTime utcDateTime)
		=> TimeZoneInfo.ConvertTimeFromUtc(
			DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
			BrasiliaTimeZone);

	private static TimeZoneInfo ResolveBrasiliaTimeZone()
	{
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
		}
		catch (TimeZoneNotFoundException)
		{
			return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
		}
	}
}

public static class PedidoMappingExtensions
{
	public static PedidoResponseDTO ToResponseDto(this Pedido pedido) => new()
	{
		Id = pedido.Id,
		DataPedido = pedido.DataPedido.ToBrasiliaTime(),
		Total = pedido.Total,
		ClienteId = pedido.ClienteId,
		ClienteNome = pedido.Cliente?.NomeCompleto ?? string.Empty,
		Status = pedido.Status,
		ConcluidoEm = pedido.ConcluidoEm?.ToBrasiliaTime(),
		DevolvidoEm = pedido.DevolvidoEm?.ToBrasiliaTime(),
		MotivoDevolucao = pedido.MotivoDevolucao,
		ItensPedidos = pedido.ItensPedidos.Select(i => new ItemPedidoResponseDTO
		{
			Id = i.Id,
			ProdutoId = i.ProdutoId,
			ProdutoNome = i.Produto?.Nome ?? string.Empty,
			Quantidade = i.Quantidade,
			PrecoUnitario = i.PrecoUnitario,
			Total = i.Total
		}).ToList()
	};
}
