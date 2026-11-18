using GerenciadorDePedidos.Core.DTOs;

namespace GerenciadorDePedidos.Application.Models;

public sealed record ClienteDetailsViewModel(
	string NomeCompleto,
	string Telefone,
	List<PedidoResponseDTO> Pedidos);