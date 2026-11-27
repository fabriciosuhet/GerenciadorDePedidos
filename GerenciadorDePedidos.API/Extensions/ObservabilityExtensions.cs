using System.Text.Json;
using GerenciadorDePedidos.API.HealthChecks;
using GerenciadorDePedidos.Core.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace GerenciadorDePedidos.API.Extensions;

public static class ObservabilityExtensions
{
	private static readonly JsonSerializerOptions HealthJsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
	{
		var isTesting = builder.Environment.IsEnvironment("Testing");

		builder.Host.UseSerilog((context, services, loggerConfiguration) =>
		{
			loggerConfiguration
				.ReadFrom.Configuration(context.Configuration)
				.ReadFrom.Services(services)
				.Enrich.FromLogContext()
				.Enrich.WithProperty("Application", AppDiagnostics.ServiceName)
				.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

			if (isTesting)
			{
				loggerConfiguration.MinimumLevel.Warning();
			}
			else
			{
				loggerConfiguration.WriteTo.Console(
					outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
			}
		});

		var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

		builder.Services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceName: AppDiagnostics.ServiceName,
				serviceVersion: typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
			.WithTracing(tracing =>
			{
				tracing
					.AddSource(AppDiagnostics.ServiceName)
					.AddSource("MassTransit")
					.AddAspNetCoreInstrumentation(opt =>
					{
						opt.RecordException = true;
						opt.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
					})
					.AddHttpClientInstrumentation();

				if (!string.IsNullOrWhiteSpace(otlpEndpoint))
					tracing.AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint));
			})
			.WithMetrics(metrics =>
			{
				metrics
					.AddMeter(AppDiagnostics.ServiceName)
					.AddMeter("MassTransit")
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation();

				if (!string.IsNullOrWhiteSpace(otlpEndpoint))
					metrics.AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint));
			});

		return builder;
	}

	public static IServiceCollection AddAppHealthChecks(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var healthChecks = services.AddHealthChecks()
			.AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

		var redisConnection = configuration.GetConnectionString("Redis");
		var redisEnabled = configuration.GetValue("Redis:Enabled", !string.IsNullOrWhiteSpace(redisConnection));

		if (redisEnabled && !string.IsNullOrWhiteSpace(redisConnection))
			healthChecks.AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);

		return services;
	}

	public static WebApplication UseObservabilityRequestLogging(this WebApplication app)
	{
		if (app.Environment.IsEnvironment("Testing"))
			return app;

		app.UseSerilogRequestLogging(options =>
		{
			options.GetLevel = (httpContext, _, ex) =>
			{
				if (ex is not null || httpContext.Response.StatusCode >= 500)
					return LogEventLevel.Error;

				if (httpContext.Request.Path.StartsWithSegments("/health"))
					return LogEventLevel.Debug;

				return LogEventLevel.Information;
			};

			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
				diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
			};
		});

		return app;
	}

	public static WebApplication MapAppHealthChecks(this WebApplication app)
	{
		app.MapHealthChecks("/health", new HealthCheckOptions
		{
			Predicate = _ => false,
			ResponseWriter = WriteHealthResponseAsync
		}).DisableRateLimiting();

		app.MapHealthChecks("/health/ready", new HealthCheckOptions
		{
			Predicate = check => check.Tags.Contains("ready"),
			ResponseWriter = WriteHealthResponseAsync
		}).DisableRateLimiting();

		return app;
	}

	private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
	{
		var logger = context.RequestServices
			.GetService<ILoggerFactory>()?
			.CreateLogger("HealthChecks");

		foreach (var (name, entry) in report.Entries)
		{
			if (entry.Exception is not null)
				logger?.LogError(entry.Exception, "Falha no health check {HealthCheckName}.", name);
		}

		context.Response.ContentType = "application/json; charset=utf-8";

		var payload = new
		{
			status = report.Status.ToString(),
			totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
			timestampUtc = DateTime.UtcNow,
			checks = report.Entries.Select(entry => new
			{
				name = entry.Key,
				status = entry.Value.Status.ToString(),
				durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
				description = entry.Value.Status == HealthStatus.Healthy
					? entry.Value.Description
					: "Dependência indisponível."
			})
		};

		return context.Response.WriteAsync(JsonSerializer.Serialize(payload, HealthJsonOptions));
	}
}

