using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreateProduto;

namespace GerenciadorDePedidos.Application.Validators;

public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
	public CreateProdutoCommandValidator()
	{
		RuleFor(p => p.Nome)
			.NotNull().WithMessage("O nome nao pode ser nulo")
			.NotEmpty().WithMessage("O nome nao pode ser vazio.")
			.MaximumLength(255)
			.WithMessage("Tamanho maximo de nome é de 255 caracteres");

		RuleFor(p => p.Preco)
			.NotNull()
			.NotEmpty()
			.GreaterThan(0).WithMessage("O valor deve ser maior que 0")
			.WithMessage("Nao pode ser vazio");
		
		RuleFor(p => p.Estoque)
			.NotNull()
			.NotEmpty().WithMessage("O estoque nao deve ser vazio nem nulo")
			.GreaterThan(0).WithMessage("O estoque deve ser maior que 0");
	}
}