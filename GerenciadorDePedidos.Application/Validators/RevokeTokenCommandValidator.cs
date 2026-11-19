using FluentValidation;
using GerenciadorDePedidos.Application.Commands.RevokeToken;

namespace GerenciadorDePedidos.Application.Validators;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
	public RevokeTokenCommandValidator()
	{
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("O refresh token não pode ser vazio.");
	}
}