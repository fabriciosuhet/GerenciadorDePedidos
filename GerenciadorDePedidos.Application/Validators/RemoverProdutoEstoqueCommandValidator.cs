using FluentValidation;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

namespace GerenciadorDePedidos.Application.Validators;

public class RemoverProdutoEstoqueCommandValidator : AbstractValidator<RemoverProdutoEstoqueCommand>
{
	public RemoverProdutoEstoqueCommandValidator()
	{
		RuleFor(re => re.ProdutoId)
			.GreaterThan(0).WithMessage("O ProdutoId deve ser maior que zero.");

		RuleFor(re => re.Quantidade)
			.GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.")
			.LessThanOrEqualTo(100_000).WithMessage("A quantidade não pode exceder 100.000 unidades por operação.");
	}
}