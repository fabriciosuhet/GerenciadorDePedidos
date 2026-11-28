using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public sealed record PedidoStatusAtualizadoDTO(
	int PedidoId,
	Guid ClienteId,
	Status StatusAnterior,
	Status StatusAtual,
	decimal Total,
	DateTime AtualizadoEm,
	string? MotivoDevolucao = null);

