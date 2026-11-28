using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Application.Models;

public sealed record DevolverPedidoViewModel(
	int PedidoId,
	Status Status,
	string Motivo,
	DateTime DevolvidoEm,
	bool EstoqueEstornado,
	int UnidadesEstornadas);

