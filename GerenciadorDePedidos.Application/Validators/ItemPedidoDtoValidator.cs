using FluentValidation;
using GerenciadorDePedidos.Core.DTOs;

namespace GerenciadorDePedidos.Application.Validators;

public class ItemPedidoDtoValidator : AbstractValidator<ItemPedidoDTO>
{
	public ItemPedidoDtoValidator()
	{
		RuleFor(ip => ip.ProdutoId)
			.GreaterThan(0)
			.WithMessage("O ID do produto deve ser maior que zero.");

		RuleFor(ip => ip.Quantidade)
			.GreaterThan(0)
			.WithMessage("A quantidade deve ser maior que zero.")
			.LessThanOrEqualTo(10_000)
			.WithMessage("A quantidade por item não pode exceder 10.000 unidades.");
	}
}