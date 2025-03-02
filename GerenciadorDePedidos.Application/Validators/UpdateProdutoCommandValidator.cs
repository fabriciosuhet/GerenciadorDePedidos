using FluentValidation;
using GerenciadorDePedidos.Application.Commands;

namespace GerenciadorDePedidos.Application.Validators;

public class UpdateProdutoCommandValidator : AbstractValidator<UpdateProdutoCommand>
{
	public UpdateProdutoCommandValidator()
	{
		RuleFor(up => up.Id)
			.NotNull().WithMessage("O ID nao pode ser nulo")
			.NotEmpty().WithMessage("O ID nao pode ser vazio");

		RuleFor(up => up.Nome)
			.NotNull().WithMessage("O nome nao pode ser nulo")
			.NotEmpty().WithMessage("O nome nao pode ser vazio")
			.MaximumLength(255).WithMessage("O nome nao pode ter mais de 255 caracteres");

		RuleFor(up => up.Preco)
			.NotNull().WithMessage("O preco nao pode ser nulo")
			.NotEmpty().WithMessage("O preco nao pode ser vazio")
			.GreaterThanOrEqualTo(0).WithMessage("O preco deve ser maior ou igual a 0");
	}
}