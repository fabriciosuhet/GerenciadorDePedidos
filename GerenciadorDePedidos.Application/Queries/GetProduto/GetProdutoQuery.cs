using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetProduto;

public sealed record GetProdutoQuery(int Id) : IRequest<ProdutoDetailsViewModel?>;