using System.Data;
using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreatePedido;

namespace GerenciadorDePedidos.Application.Validators;

public class CreatePedidoCommandValidator : AbstractValidator<CreatePedidoCommand>
{
	public CreatePedidoCommandValidator()
	{
		RuleFor(p => p.Total)
			.GreaterThan(0).WithMessage("O valor deve ser maior que 0")
			.NotNull()
			.WithMessage("O valor nao pode ser nulo");
	}
}