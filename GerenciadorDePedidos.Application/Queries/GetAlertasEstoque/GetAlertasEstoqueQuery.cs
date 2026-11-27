using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAlertasEstoque;

public sealed record GetAlertasEstoqueQuery(bool? ApenasPendentes = true) : IRequest<List<AlertaEstoqueViewModel>>;

