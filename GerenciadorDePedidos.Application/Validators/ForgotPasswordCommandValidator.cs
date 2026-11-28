using FluentValidation;
using GerenciadorDePedidos.Application.Commands.ForgotPassword;

namespace GerenciadorDePedidos.Application.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
	public ForgotPasswordCommandValidator()
	{
		RuleFor(x => x.Email).ApplyEmailRules();
	}
}

