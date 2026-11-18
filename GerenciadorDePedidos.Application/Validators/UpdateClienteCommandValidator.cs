using FluentValidation;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
{
	public UpdateClienteCommandValidator()
	{
		RuleFor(uc => uc.Id)
			.NotEmpty().WithMessage("O id não pode ser vazio.");

		RuleFor(uc => uc.Email)
			.NotEmpty().WithMessage("O Email não pode ser vazio.")
			.EmailAddress().WithMessage("Email não válido.")
			.MaximumLength(255).WithMessage("O email não deve ser maior que 255 caracteres.");

		RuleFor(uc => uc.Telefone)
			.NotEmpty().WithMessage("O telefone não pode ser vazio.")
			.MaximumLength(20).WithMessage("O telefone não deve ter mais de 20 caracteres.");
	}
}