using System.Text.RegularExpressions;
using FluentValidation;
using GerenciadorDePedidos.Application.Commands.CreateCliente;

namespace GerenciadorDePedidos.Application.Validators;

public class CreateClineteCommandValidator : AbstractValidator<CreateClienteCommand>
{
	public CreateClineteCommandValidator()
	{
		RuleFor(cc => cc.Email).EmailAddress().WithMessage("Email nao valido.");

		RuleFor(cc => cc.Senha)
			.Must(SenhaValida)
			.WithMessage(
				"A senha precisa conter 8 caracteres, 1 numero, 1 letra maiscula, 1 letra minuscula e 1 caractere especial");

		RuleFor(cc => cc.NomeCompleto)
			.NotNull().NotEmpty().WithMessage("Nome obrigatorio"); 

	}

	public bool SenhaValida(string senha)
	{
		var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
		return regex.IsMatch(senha);
	}
	
}