using FluentValidation;
using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

namespace GerenciadorDePedidos.Application.Validators;

public class AdicionarProdutoEstoqueCommandValidator : AbstractValidator<AdicionarProdutoEstoqueCommand>
{
	public AdicionarProdutoEstoqueCommandValidator()
	{
		RuleFor(ap => ap.ProdutoId)
			.GreaterThan(0).WithMessage("O ProdutoId deve ser maior que zero.");

		RuleFor(ap => ap.Quantidade)
			.GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

		RuleFor(ap => ap.ClienteId)
			.NotEmpty().WithMessage("O ClienteId não pode ser vazio.");
	}
}