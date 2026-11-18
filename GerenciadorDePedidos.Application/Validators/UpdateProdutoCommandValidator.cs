using FluentValidation;
using GerenciadorDePedidos.Application.Commands.UpdateProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class UpdateProdutoCommandValidator : AbstractValidator<UpdateProdutoCommand>
{
	public UpdateProdutoCommandValidator()
	{
		RuleFor(up => up.Id)
			.GreaterThan(0).WithMessage("O ID deve ser maior que zero.");

		RuleFor(up => up.Nome)
			.NotEmpty().WithMessage("O nome não pode ser vazio.")
			.MaximumLength(255).WithMessage("O nome não pode ter mais de 255 caracteres.");

		RuleFor(up => up.Preco)
			.GreaterThanOrEqualTo(0).WithMessage("O preço deve ser maior ou igual a 0.");

		RuleFor(up => up.Estoque)
			.GreaterThanOrEqualTo(0).WithMessage("O estoque deve ser maior ou igual a 0.");
	}
}