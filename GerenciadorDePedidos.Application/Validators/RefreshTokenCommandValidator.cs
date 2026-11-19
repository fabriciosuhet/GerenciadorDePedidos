using FluentValidation;
using GerenciadorDePedidos.Application.Commands.RefreshToken;

namespace GerenciadorDePedidos.Application.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
	public RefreshTokenCommandValidator()
	{
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("O refresh token não pode ser vazio.");
	}
}