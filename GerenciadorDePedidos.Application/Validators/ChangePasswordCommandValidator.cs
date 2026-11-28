using FluentValidation;
using GerenciadorDePedidos.Application.Commands.ChangePassword;

namespace GerenciadorDePedidos.Application.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
	public ChangePasswordCommandValidator()
	{
		RuleFor(x => x.SenhaAtual)
			.NotEmpty().WithMessage("A senha atual é obrigatória.")
			.MaximumLength(128).WithMessage("A senha atual não pode ter mais que 128 caracteres.");

		RuleFor(x => x.NovaSenha).ApplySenhaRules();

		RuleFor(x => x.ConfirmarNovaSenha)
			.NotEmpty().WithMessage("A confirmação de senha é obrigatória.")
			.Equal(x => x.NovaSenha).WithMessage("A confirmação de senha não confere com a nova senha.");
	}
}

