namespace GerenciadorDePedidos.API.Helpers;

public static class CacheKeyHelper
{
	public static string GetAllClientesKey(string? query)
	{
		return $"clientes:{ query ?? "all"}";
	}

	public static string GetClienteByIdKey(Guid id)
	{
		return $"cliente:{id}";
	}
	
	public static string GetAllProdutosKey(string? query)
	{
		return $"produtos:{ query ?? "all"}";
	}

	public static string GetProdutoByIdKey(Guid id)
	{
		return $"produto:{id}";
	}
	
	public static string GetAllPedidosKey(string? query)
	{
		return $"pedidos:{ query ?? "all"}";
	}

	public static string GetPedidoByIdKey(Guid id)
	{
		return $"pedido:{id}";
	}
}