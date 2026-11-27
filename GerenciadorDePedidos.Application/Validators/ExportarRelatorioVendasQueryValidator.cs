using FluentValidation;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioVendas;

namespace GerenciadorDePedidos.Application.Validators;

public sealed class ExportarRelatorioVendasQueryValidator : AbstractValidator<ExportarRelatorioVendasQuery>
{
	public ExportarRelatorioVendasQueryValidator()
	{
		RuleFor(x => x.Periodo)
			.IsInEnum()
			.WithMessage("Período inválido. Utilize: Semana, Mes ou Ano.");

		RuleFor(x => x.Formato)
			.IsInEnum()
			.WithMessage("Formato inválido. Utilize: Csv ou Pdf.");
	}
}

