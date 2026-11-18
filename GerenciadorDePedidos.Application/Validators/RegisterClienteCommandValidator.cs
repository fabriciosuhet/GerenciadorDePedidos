using FluentValidation;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class RegisterClienteCommandValidator : AbstractValidator<RegisterClienteCommand>
{
	public RegisterClienteCommandValidator()
	{
		RuleFor(rc => rc.Email).ApplyEmailRules();
		RuleFor(rc => rc.Senha).ApplySenhaRules();
		RuleFor(rc => rc.NomeCompleto).ApplyNomeRules();
		RuleFor(rc => rc.Telefone).ApplyTelefoneRules();
	}
}
