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
			.GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.")
			.LessThanOrEqualTo(100_000).WithMessage("A quantidade não pode exceder 100.000 unidades por operação.");
	}
}