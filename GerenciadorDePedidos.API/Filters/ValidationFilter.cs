using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GerenciadorDePedidos.API.Filters;

public class ValidationFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		throw new NotImplementedException();
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		if (!context.ModelState.IsValid)
		{
				var messages = context.ModelState
					.SelectMany(ms => ms.Value.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();
				context.Result = new BadRequestObjectResult(messages);
		}
	}
}