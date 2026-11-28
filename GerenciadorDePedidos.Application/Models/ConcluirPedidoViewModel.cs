using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Application.Models;

public sealed record ConcluirPedidoViewModel(
	int PedidoId,
	Status Status,
	DateTime ConcluidoEm);

