using FluentValidation;
using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;

namespace GerenciadorDePedidos.Application.Validators;

public class AdicionarProdutoEstoqueCommandValidator : AbstractValidator<AdicionarProdutoEstoqueCommand>
{
	public AdicionarProdutoEstoqueCommandValidator()
	{
		RuleFor(ap => ap.ProdutoId)
			.NotEmpty()
			.WithMessage("O produtoId nao pode ser vazio")
			.Must(id => id != Guid.Empty)
			.WithMessage("O ProdutoId deve ser um valor válido (diferente de Guid.Empty).");

		RuleFor(ap => ap.Quantidade)
			.GreaterThanOrEqualTo(0)
			.WithMessage("A quantidade deve ser maior ou igual a 0");
		
		RuleFor(ap => ap.ClienteId)
			.NotEmpty()
			.WithMessage("O ClienteId nao pode ser vazio")
			.Must(id => id != Guid.Empty)
			.WithMessage("O ClienteId deve ser um valor válido (diferente de Guid.Empty).");
	}
}