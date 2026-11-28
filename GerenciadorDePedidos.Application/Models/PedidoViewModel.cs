using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Application.Models;

public sealed record PedidoViewModel(int Id, DateTime DataPedido, decimal Total, Status Status = Status.Pendente);