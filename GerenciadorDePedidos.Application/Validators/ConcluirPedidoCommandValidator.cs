using FluentValidation;
using GerenciadorDePedidos.Application.Commands.ConcluirPedido;

namespace GerenciadorDePedidos.Application.Validators;

public class ConcluirPedidoCommandValidator : AbstractValidator<ConcluirPedidoCommand>
{
	public ConcluirPedidoCommandValidator()
	{
		RuleFor(x => x.PedidoId)
			.GreaterThan(0).WithMessage("O identificador do pedido deve ser maior que zero.");
	}
}

