using System.Diagnostics.Metrics;
using GerenciadorDePedidos.Core.Observability;

namespace GerenciadorDePedidos.UnitTests.Core.Observability;

public class AppDiagnosticsTests
{
	[Fact]
	public void AppDiagnostics_DeveEmitirMetricasDeNegocio()
	{
		var contagens = new Dictionary<string, long>();

		using var listener = new MeterListener();
		listener.InstrumentPublished = (instrument, meterListener) =>
		{
			if (instrument.Meter.Name == AppDiagnostics.ServiceName)
				meterListener.EnableMeasurementEvents(instrument);
		};
		listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
		{
			contagens[instrument.Name] = contagens.GetValueOrDefault(instrument.Name) + measurement;
		});
		listener.Start();

		AppDiagnostics.PedidosCriados.Add(1);
		AppDiagnostics.PedidosConcluidos.Add(2);
		AppDiagnostics.PedidosCancelados.Add(1);
		AppDiagnostics.AlertasEstoqueBaixo.Add(3);

		Assert.Equal(1, contagens["gerenciador_pedidos_criados_total"]);
		Assert.Equal(2, contagens["gerenciador_pedidos_concluidos_total"]);
		Assert.Equal(1, contagens["gerenciador_pedidos_cancelados_total"]);
		Assert.Equal(3, contagens["gerenciador_alertas_estoque_baixo_total"]);
	}
}

