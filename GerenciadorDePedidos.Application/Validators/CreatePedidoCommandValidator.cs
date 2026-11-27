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
			.WithMessage("A lista de itens pedidos deve conter pelo menos um item")
			.Must(itens => itens is null || itens.Count <= 100)
			.WithMessage("O pedido não pode conter mais de 100 itens.");

		RuleForEach(p => p.ItensPedidos)
			.SetValidator(new ItemPedidoDtoValidator());
	}
	
	
}