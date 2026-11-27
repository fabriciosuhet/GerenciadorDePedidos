using System.Text.RegularExpressions;
using FluentValidation;

namespace GerenciadorDePedidos.Application.Validators;

internal static partial class ClienteValidationRules
{
	public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> rule)
		=> rule.NotEmpty().WithMessage("O email não pode ser vazio.")
			.MaximumLength(150).WithMessage("O email não pode ter mais que 150 caracteres.")
			.EmailAddress().WithMessage("Email não válido.");

	public static IRuleBuilderOptions<T, string> ApplySenhaRules<T>(this IRuleBuilder<T, string> rule)
		=> rule.NotEmpty().WithMessage("A senha não pode ser vazia.")
			.MaximumLength(128).WithMessage("A senha não pode ter mais que 128 caracteres.")
			.Must(SenhaValida)
			.WithMessage("A senha precisa conter 8 caracteres, 1 número, 1 letra maiúscula, 1 letra minúscula e 1 caractere especial.");

	public static IRuleBuilderOptions<T, string> ApplyNomeRules<T>(this IRuleBuilder<T, string> rule)
		=> rule.NotEmpty().WithMessage("Nome obrigatório.")
			.MaximumLength(150).WithMessage("O nome não pode ter mais que 150 caracteres.")
			.Must(nome => !string.IsNullOrEmpty(nome) && !nome.Any(char.IsControl))
			.WithMessage("O nome contém caracteres inválidos.");

	public static IRuleBuilderOptions<T, string> ApplyTelefoneRules<T>(this IRuleBuilder<T, string> rule)
		=> rule.NotEmpty().WithMessage("Telefone obrigatório.")
			.MaximumLength(20).WithMessage("O telefone não pode ter mais que 20 caracteres.");

	private static bool SenhaValida(string? senha)
		=> !string.IsNullOrEmpty(senha) && SenhaRegex().IsMatch(senha);

	[GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
	private static partial Regex SenhaRegex();
}
