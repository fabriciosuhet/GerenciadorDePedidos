using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.Application.Commands.CancelarPedido;
using GerenciadorDePedidos.Application.Commands.ConcluirPedido;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Commands.DevolverPedido;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioVendas;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/pedido")]
[AcessoPadrao]
public class PedidoController(
	IMediator mediator,
	ICacheService cacheService,
	ICurrentUserService currentUserService)
	: ApiControllerBase(mediator, cacheService)
{
	[HttpGet]
	public Task<IActionResult> GetAll(
		[FromQuery] string? query,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 10,
		CancellationToken cancellationToken = default)
	{
		var (pagina, tamanho) = NormalizarPaginacao(pageNumber, pageSize);
		var clienteScope = currentUserService.IsAdmin ? null : currentUserService.ClienteId;
		return GetCachedOrNotFoundAsync<PagedResultModel<PedidoViewModel>>(
			CacheKeyHelper.GetAllPedidosKey(query, clienteScope, pagina, tamanho),
			new GetAllPedidosQuery(query, pagina, tamanho),
			"Pedidos nao encontrados",
			cancellationToken);
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
	{
		var cacheKey = CacheKeyHelper.GetPedidoByIdKey(id);
		if (CacheService is not null && CacheService.TryGet<PedidoResponseDTO>(cacheKey, out var cached) && cached is not null)
		{
			if (!currentUserService.IsAdmin && cached.ClienteId != currentUserService.ClienteId)
				return Forbid();

			return Ok(cached);
		}

		return await GetCachedOrNotFoundAsync<PedidoResponseDTO>(
			cacheKey,
			new GetPedidoQuery(id),
			"Pedido nao encontrado",
			cancellationToken);
	}

	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CreatePedidoCommand command, CancellationToken cancellationToken)
	{
		var pedidoId = await SendAsync(command, cancellationToken);
		CacheService?.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		CacheService?.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(command.ClienteId));
		CacheService?.Remove(CacheKeyHelper.GetClienteByIdKey(command.ClienteId));
		return CreatedAtAction(nameof(GetById), new { id = pedidoId }, new { id = pedidoId });
	}

	[HttpGet("relatorio")]
	[AcessoRestrito]
	public async Task<IActionResult> DownloadRelatorioVendas(
		[FromQuery] PeriodoRelatorio periodo = PeriodoRelatorio.Mes,
		[FromQuery] FormatoRelatorio formato = FormatoRelatorio.Pdf,
		[FromQuery] DateTime? dataReferencia = null,
		CancellationToken cancellationToken = default)
	{
		var arquivo = await SendAsync(
			new ExportarRelatorioVendasQuery(periodo, formato, dataReferencia),
			cancellationToken);

		return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
	}

	[HttpPatch("{id:int}/cancelar")]
	public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
	{
		var resultado = await SendAsync(new CancelarPedidoCommand(id), cancellationToken);
		return Ok(resultado);
	}

	[HttpPatch("{id:int}/concluir")]
	[AcessoRestrito]
	public async Task<IActionResult> Concluir(int id, CancellationToken cancellationToken)
	{
		var resultado = await SendAsync(new ConcluirPedidoCommand(id), cancellationToken);
		return Ok(resultado);
	}

	[HttpPatch("{id:int}/devolver")]
	[AcessoRestrito]
	public async Task<IActionResult> Devolver(
		int id,
		[FromBody] DevolverPedidoRequest request,
		CancellationToken cancellationToken)
	{
		var resultado = await SendAsync(
			new DevolverPedidoCommand(id, request.Motivo, request.RetornarAoEstoque),
			cancellationToken);
		return Ok(resultado);
	}

	[HttpDelete("deletar-pedido")]
	[AcessoRestrito]
	public async Task<IActionResult> Delete([FromQuery] int id, CancellationToken cancellationToken)
	{
		await SendAsync(new DeletePedidoCommand(id), cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetPedidoByIdKey(id));
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllPedidosPrefix);
		return Ok("pedido deletado com sucesso");
	}
}

public sealed record DevolverPedidoRequest(string Motivo, bool RetornarAoEstoque = true);