using System.Diagnostics;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GerenciadorDePedidos.Infrastructure.Messaging;

public sealed class CorrelationIdConsumeFilter<T>(
	ICorrelationContext correlationContext,
	ILogger<CorrelationIdConsumeFilter<T>> logger) : IFilter<ConsumeContext<T>>
	where T : class
{
	public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
	{
		var correlationId = context.Headers.Get<string>(AppDiagnostics.CorrelationIdHeader)
			?? context.CorrelationId?.ToString("D")
			?? Guid.NewGuid().ToString("D");

		Activity.Current?.SetTag("correlation.id", correlationId);

		using var correlationScope = correlationContext.BeginScope(correlationId);
		using var logScope = logger.BeginScope(new Dictionary<string, object>
		{
			["CorrelationId"] = correlationId,
			["MessageType"] = typeof(T).Name
		});

		await next.Send(context);
	}

	public void Probe(ProbeContext context)
		=> context.CreateFilterScope("correlationId");
}

