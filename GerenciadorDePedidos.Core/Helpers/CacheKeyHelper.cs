namespace GerenciadorDePedidos.Core.Helpers;

public static class CacheKeyHelper
{
	public const string AllClientesPrefix = "clientes:";
	public const string AllProdutosPrefix = "produtos:";
	public const string AllPedidosPrefix = "pedidos:";

	public static string GetAllClientesKey(string? query, int pageNumber = 1, int pageSize = 10)
		=> $"{AllClientesPrefix}{query ?? "all"}:{pageNumber}:{pageSize}";

	public static string GetClienteByIdKey(Guid id) => $"cliente:{id}";

	public static string GetAllProdutosKey(string? query, int pageNumber = 1, int pageSize = 10)
		=> $"{AllProdutosPrefix}{query ?? "all"}:{pageNumber}:{pageSize}";

	public static string GetProdutoByIdKey(int id) => $"produto:{id}";

	public static string GetAllPedidosKey(string? query, Guid? clienteId = null, int pageNumber = 1, int pageSize = 10)
		=> $"{AllPedidosPrefix}{(clienteId.HasValue ? clienteId.Value.ToString() : "admin")}:{query ?? "all"}:{pageNumber}:{pageSize}";

	public static string GetAllPedidosAdminPrefix() => $"{AllPedidosPrefix}admin:";
	public static string GetAllPedidosClientePrefix(Guid clienteId) => $"{AllPedidosPrefix}{clienteId}:";
	public static string GetPedidoByIdKey(int id) => $"pedido:{id}";
}
