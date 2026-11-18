using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace GerenciadorDePedidos.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var (statusCode, payload) = exception switch
		{
			ValidationException validationEx => (
				StatusCodes.Status400BadRequest,
				(object)new { errors = validationEx.Errors.Select(e => e.ErrorMessage).Distinct().ToList() }),

			ArgumentException argEx => (
				StatusCodes.Status400BadRequest,
				new { error = argEx.Message }),

			KeyNotFoundException notFoundEx => (
				StatusCodes.Status404NotFound,
				new { error = notFoundEx.Message }),

			UnauthorizedAccessException unauthorizedEx => (
				httpContext.User.Identity?.IsAuthenticated == true
					? StatusCodes.Status403Forbidden
					: StatusCodes.Status401Unauthorized,
				new { error = unauthorizedEx.Message }),

			_ => (
				StatusCodes.Status500InternalServerError,
				new { error = "Ocorreu um erro interno no servidor." })
		};

		if (statusCode == StatusCodes.Status500InternalServerError)
			logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

		httpContext.Response.StatusCode = statusCode;
		await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
		return true;
	}
}
