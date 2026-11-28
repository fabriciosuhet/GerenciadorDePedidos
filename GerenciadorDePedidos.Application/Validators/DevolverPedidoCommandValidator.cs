using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DevolverPedido;

namespace GerenciadorDePedidos.Application.Validators;

public class DevolverPedidoCommandValidator : AbstractValidator<DevolverPedidoCommand>
{
	public DevolverPedidoCommandValidator()
	{
		RuleFor(x => x.PedidoId)
			.GreaterThan(0).WithMessage("O identificador do pedido deve ser maior que zero.");

		RuleFor(x => x.Motivo)
			.NotEmpty().WithMessage("O motivo da devolução é obrigatório.")
			.MaximumLength(500).WithMessage("O motivo da devolução não pode exceder 500 caracteres.");
	}
}

