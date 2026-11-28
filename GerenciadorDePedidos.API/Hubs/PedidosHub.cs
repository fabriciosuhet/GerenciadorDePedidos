using System.Security.Claims;
using GerenciadorDePedidos.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GerenciadorDePedidos.API.Hubs;

[Authorize]
public sealed class PedidosHub(ILogger<PedidosHub> logger) : Hub
{
	public const string Route = "/hubs/pedidos";
	public const string AdminsGroup = "admins";
	public const string EventoPedidoStatusAtualizado = "PedidoStatusAtualizado";

	public static string ClienteGroup(Guid clienteId) => $"cliente:{clienteId:D}";

	public override async Task OnConnectedAsync()
	{
		var rawClienteId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
			?? Context.User?.FindFirstValue("clienteId");

		if (Guid.TryParse(rawClienteId, out var clienteId))
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, ClienteGroup(clienteId));
		}

		if (Context.User?.IsInRole(nameof(Role.Admin)) == true)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
		}

		logger.LogDebug(
			"Conexão SignalR {ConnectionId} autenticada no PedidosHub (IsAdmin={IsAdmin}).",
			Context.ConnectionId,
			Context.User?.IsInRole(nameof(Role.Admin)) == true);

		await base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		logger.LogDebug(
			exception,
			"Conexão SignalR {ConnectionId} desconectada do PedidosHub.",
			Context.ConnectionId);

		return base.OnDisconnectedAsync(exception);
	}
}

