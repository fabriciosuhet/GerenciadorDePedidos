using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Commands.DeleteProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class DeleteProducoCommandValidator : AbstractValidator<DeleteProdutoCommand>
{
	public DeleteProducoCommandValidator()
	{
		RuleFor(p => p.Id)
			.NotNull().WithMessage("O id nao pode ser nulo")
			.NotEmpty().WithMessage("O id nao pode ser vazio");
	}
}