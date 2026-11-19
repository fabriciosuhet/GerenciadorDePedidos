namespace GerenciadorDePedidos.Core.Helpers;

public static class CacheKeyHelper
{
	public static string GetAllClientesKey(string? query) => $"clientes:{query ?? "all"}";
	public static string GetClienteByIdKey(Guid id) => $"cliente:{id}";
	public static string GetAllProdutosKey(string? query) => $"produtos:{query ?? "all"}";
	public static string GetProdutoByIdKey(int id) => $"produto:{id}";
	public static string GetAllPedidosKey(string? query) => $"pedidos:{query ?? "all"}";
	public static string GetPedidoByIdKey(int id) => $"pedido:{id}";
}
