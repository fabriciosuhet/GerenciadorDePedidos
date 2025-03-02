using FluentValidation;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
{
	public UpdateClienteCommandValidator()
	{
		RuleFor(uc => uc.Id)
			.NotNull().WithMessage("O id nao pode ser nulo")
			.NotEmpty().WithMessage("O id nao pode ser vazio")
			.WithMessage("Insira um ID valido");
		
		RuleFor(uc => uc.Email)
			.NotNull().WithMessage("O Email nao pode ser nulo")
			.NotEmpty().WithMessage("O Email nao pode ser vazio")
			.MaximumLength(255).WithMessage("O email nao deve ser maior que 255 caracteres");

		RuleFor(uc => uc.Telefone)
			.NotNull().WithMessage("O Telefone nao pode ser nulo")
			.NotEmpty().WithMessage("O telefone nao pode ser vazio")
			.MaximumLength(11).WithMessage("O telefone nao deve ter mais de 11 digitos");
	}
}