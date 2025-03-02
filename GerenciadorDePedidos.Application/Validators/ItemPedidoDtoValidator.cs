using FluentValidation;
using GerenciadorDePedidos.Core.DTOs;

namespace GerenciadorDePedidos.Application.Validators;

public class ItemPedidoDtoValidator : AbstractValidator<ItemPedidoDTO>
{
	public ItemPedidoDtoValidator()
	{
		RuleFor(ip => ip.produtoId)
			.NotEmpty()
			.WithMessage("O ID do produto nao pode ser vazio nem nulo");

		RuleFor(ip => ip.Quantidade)
			.GreaterThan(0)
			.WithMessage("A quantidade deve ser maior que zero");
		
	}
}