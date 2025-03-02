using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DeletePedido;

namespace GerenciadorDePedidos.Application.Validators;

public class DeletePedidoCommandValidator : AbstractValidator<DeletePedidoCommand>
{
	public DeletePedidoCommandValidator()
	{
		RuleFor(p => p.Id)
			.NotNull().WithMessage("O id nao pode ser nulo")
			.NotEmpty().WithMessage("O id nao pode ser vazio");
	}
}