using FluentValidation;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioMovimentacaoEstoque;

namespace GerenciadorDePedidos.Application.Validators;

public sealed class ExportarRelatorioMovimentacaoEstoqueQueryValidator
	: AbstractValidator<ExportarRelatorioMovimentacaoEstoqueQuery>
{
	public ExportarRelatorioMovimentacaoEstoqueQueryValidator()
	{
		RuleFor(x => x.Periodo)
			.IsInEnum()
			.WithMessage("Período inválido. Utilize: Semana, Mes ou Ano.");

		RuleFor(x => x.Formato)
			.IsInEnum()
			.WithMessage("Formato inválido. Utilize: Csv ou Pdf.");
	}
}

