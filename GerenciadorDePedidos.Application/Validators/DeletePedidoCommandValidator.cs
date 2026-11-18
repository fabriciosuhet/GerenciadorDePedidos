using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DeletePedido;

namespace GerenciadorDePedidos.Application.Validators;

public class DeletePedidoCommandValidator : AbstractValidator<DeletePedidoCommand>
{
	public DeletePedidoCommandValidator()
	{
		RuleFor(p => p.Id)
			.GreaterThan(0).WithMessage("O id deve ser maior que zero.");
	}
}