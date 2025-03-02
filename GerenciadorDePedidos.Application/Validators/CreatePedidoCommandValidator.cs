using System.Data;
using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreatePedido;

namespace GerenciadorDePedidos.Application.Validators;

public class CreatePedidoCommandValidator : AbstractValidator<CreatePedidoCommand>
{
	public CreatePedidoCommandValidator()
	{
		RuleFor(p => p.ClienteId)
			.NotEmpty()
			.WithMessage("O ID do cliente nao pode ser vazio ou nulo");

		RuleFor(p => p.ItensPedidos)
			.NotNull()
			.WithMessage("A lista de pedidos nao pode ser nula")
			.NotEmpty()
			.WithMessage("A lista de itens pedidos deve conter pelo menos um item");

		RuleForEach(p => p.ItensPedidos)
			.SetValidator(new ItemPedidoDtoValidator());
	}
	
	
}