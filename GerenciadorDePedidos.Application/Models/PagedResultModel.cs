namespace GerenciadorDePedidos.Application.Models;

public class PagedResultModel<T>
{
	public List<T> Items { get; set; }
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }

	public PagedResultModel(List<T> items, int totalCount, int pageNumber, int pageSize)
	{
		Items = items;
		TotalCount = totalCount;
		PageNumber = pageNumber;
		PageSize = pageSize;
	}
}