using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreateCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
	public CreateClienteCommandValidator()
	{
		RuleFor(cc => cc.Email).ApplyEmailRules();
		RuleFor(cc => cc.Senha).ApplySenhaRules();
		RuleFor(cc => cc.NomeCompleto).ApplyNomeRules();
		RuleFor(cc => cc.Telefone).ApplyTelefoneRules();
	}
}
