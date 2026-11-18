using FluentValidation;
using GerenciadorDePedidos.Application.Commands.DeleteProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class DeleteProdutoCommandValidator : AbstractValidator<DeleteProdutoCommand>
{
	public DeleteProdutoCommandValidator()
	{
		RuleFor(p => p.Id)
			.GreaterThan(0).WithMessage("O id deve ser maior que zero.");
	}
}