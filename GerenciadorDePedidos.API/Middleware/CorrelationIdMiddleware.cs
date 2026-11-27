using System.Diagnostics;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Services;
using Serilog.Context;

namespace GerenciadorDePedidos.API.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext httpContext, ICorrelationContext correlationContext)
	{
		var incomingHeader = httpContext.Request.Headers[AppDiagnostics.CorrelationIdHeader].FirstOrDefault();
		var correlationId = !string.IsNullOrWhiteSpace(incomingHeader) && incomingHeader.Length <= 128
			? incomingHeader.Trim()
			: Guid.NewGuid().ToString("D");

		httpContext.TraceIdentifier = correlationId;
		httpContext.Items[AppDiagnostics.CorrelationIdHeader] = correlationId;

		httpContext.Response.OnStarting(() =>
		{
			httpContext.Response.Headers[AppDiagnostics.CorrelationIdHeader] = correlationId;
			return Task.CompletedTask;
		});

		Activity.Current?.SetTag("correlation.id", correlationId);

		using var correlationScope = correlationContext.BeginScope(correlationId);
		using var logContextScope = LogContext.PushProperty("CorrelationId", correlationId);

		await next(httpContext);
	}
}

