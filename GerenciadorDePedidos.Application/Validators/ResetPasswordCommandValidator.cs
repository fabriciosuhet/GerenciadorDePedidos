using FluentValidation;
using GerenciadorDePedidos.Application.Commands.ResetPassword;

namespace GerenciadorDePedidos.Application.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
	public ResetPasswordCommandValidator()
	{
		RuleFor(x => x.Token)
			.NotEmpty().WithMessage("O token de redefinição de senha é obrigatório.")
			.MaximumLength(512).WithMessage("O token de redefinição de senha é inválido.");

		RuleFor(x => x.NovaSenha).ApplySenhaRules();

		RuleFor(x => x.ConfirmarNovaSenha)
			.NotEmpty().WithMessage("A confirmação de senha é obrigatória.")
			.Equal(x => x.NovaSenha).WithMessage("A confirmação de senha não confere com a nova senha.");
	}
}

