using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreateProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
	public CreateProdutoCommandValidator()
	{
		RuleFor(p => p.Nome)
			.NotEmpty().WithMessage("O nome não pode ser vazio.")
			.MaximumLength(255).WithMessage("Tamanho máximo de nome é de 255 caracteres.");

		RuleFor(p => p.Preco)
			.GreaterThan(0).WithMessage("O valor deve ser maior que 0.");

		RuleFor(p => p.Estoque)
			.GreaterThanOrEqualTo(0).WithMessage("O estoque deve ser maior ou igual a 0.");
	}
}