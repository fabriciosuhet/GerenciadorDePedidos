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
			.GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

		RuleFor(re => re.ClienteId)
			.NotEmpty().WithMessage("O ClienteId não pode ser vazio.");
	}
}