using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Application.Models;

public sealed record CancelarPedidoViewModel(
	int PedidoId,
	Status Status,
	bool EstoqueEstornado,
	int UnidadesEstornadas);

