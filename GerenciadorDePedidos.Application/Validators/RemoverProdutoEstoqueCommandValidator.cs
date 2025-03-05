using FluentValidation;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

namespace GerenciadorDePedidos.Application.Validators;

public class RemoverProdutoEstoqueCommandValidator : AbstractValidator<RemoverProdutoEstoqueCommand>
{
	public RemoverProdutoEstoqueCommandValidator()
	{
		RuleFor(re => re.ProdutoId)
			.NotEmpty()
			.WithMessage("o PedidoId nao pode ser vazio")
			.Must(id => id != Guid.Empty)
			.WithMessage("O ProdutoId deve ser um valor válido (diferente de Guid.Empty).");

		RuleFor(re => re.Quantidade)
			.NotEmpty()
			.WithMessage("O Quantidade nao pode ser vazia")
			.GreaterThanOrEqualTo(0)
			.WithMessage("A quantidade deve ser maior ou igual a zero.");

		RuleFor(re => re.ClienteId)
			.NotEmpty()
			.WithMessage("O ClienteId nao pode ser vazio")
			.Must(id => id != Guid.Empty)
			.WithMessage("O ClienteId deve ser um valor válido (diferente de Guid.Empty).");
	}
}