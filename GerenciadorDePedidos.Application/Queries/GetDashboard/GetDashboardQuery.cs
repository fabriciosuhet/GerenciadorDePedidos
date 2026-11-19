using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetDashboard;

public sealed record GetDashboardQuery : IRequest<DashboardViewModel>;
