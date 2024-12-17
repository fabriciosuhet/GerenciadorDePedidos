using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreateProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
	public CreateProdutoCommandValidator()
	{
		RuleFor(p => p.Nome).MaximumLength(255).WithMessage("Tamanho maximo de nome é de 255 caracteres");

		RuleFor(p => p.Preco).NotNull().NotEmpty().WithMessage("Nao pode ser vazio");
	}
}