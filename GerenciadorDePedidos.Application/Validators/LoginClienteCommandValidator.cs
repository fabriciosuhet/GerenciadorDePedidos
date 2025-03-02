using FluentValidation;
using GerenciadorDePedidos.Application.Commands.LoginCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class LoginClienteCommandValidator : AbstractValidator<LoginClienteCommand>
{
	public LoginClienteCommandValidator()
	{
		RuleFor(lc => lc.Email)
			.NotNull().WithMessage("O email nao pode ser nulo")
			.NotEmpty().WithMessage("O email nao pode ser vaziod")
			.EmailAddress().WithMessage("O email deve ser valido");

		RuleFor(lc => lc.Password)
			.NotNull().WithMessage("A senha nao pode ser nula")
			.NotEmpty().WithMessage("A senha nao pode ser vazia");
	}
}