using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GerenciadorDePedidos.Core.Observability;

public static class AppDiagnostics
{
	public const string ServiceName = "GerenciadorDePedidos";
	public const string CorrelationIdHeader = "X-Correlation-ID";

	public static readonly ActivitySource ActivitySource = new(ServiceName);
	public static readonly Meter Meter = new(ServiceName);

	public static readonly Counter<long> PedidosCriados =
		Meter.CreateCounter<long>("gerenciador_pedidos_criados_total", description: "Total de pedidos criados.");

	public static readonly Counter<long> PedidosConcluidos =
		Meter.CreateCounter<long>("gerenciador_pedidos_concluidos_total", description: "Total de pedidos concluídos.");

	public static readonly Counter<long> PedidosCancelados =
		Meter.CreateCounter<long>("gerenciador_pedidos_cancelados_total", description: "Total de pedidos cancelados.");

	public static readonly Counter<long> AlertasEstoqueBaixo =
		Meter.CreateCounter<long>("gerenciador_alertas_estoque_baixo_total", description: "Total de alertas de estoque baixo registrados.");
}

