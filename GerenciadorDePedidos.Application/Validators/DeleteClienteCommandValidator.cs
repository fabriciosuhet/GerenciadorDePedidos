using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class DeleteClienteCommandValidator : AbstractValidator<DeleteClienteCommand>
{
	public DeleteClienteCommandValidator()
	{
		RuleFor(c => c.Id)
			.NotNull().WithMessage("O id nao pode ser nulo")
			.NotEmpty().WithMessage("O id nao pode ser vazio");
	}
}