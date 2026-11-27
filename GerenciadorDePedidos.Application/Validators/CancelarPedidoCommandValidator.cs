using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CancelarPedido;

namespace GerenciadorDePedidos.Application.Validators;

public sealed class CancelarPedidoCommandValidator : AbstractValidator<CancelarPedidoCommand>
{
	public CancelarPedidoCommandValidator()
	{
		RuleFor(x => x.PedidoId)
			.GreaterThan(0)
			.WithMessage("O ID do pedido deve ser maior que zero.");
	}
}

