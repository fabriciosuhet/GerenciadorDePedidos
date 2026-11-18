namespace GerenciadorDePedidos.Application.Models;

public sealed record PagedResultModel<T>(
	List<T> Items,
	int TotalCount,
	int PageNumber,
	int PageSize);