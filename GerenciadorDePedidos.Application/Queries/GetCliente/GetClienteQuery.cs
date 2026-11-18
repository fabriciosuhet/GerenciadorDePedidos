using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetCliente;

public sealed record GetClienteQuery(Guid Id) : IRequest<ClienteDetailsViewModel?>;