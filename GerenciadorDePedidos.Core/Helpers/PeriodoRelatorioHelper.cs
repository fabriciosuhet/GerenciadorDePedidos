using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Helpers;

public static class PeriodoRelatorioHelper
{
	public static (DateTime DataInicio, DateTime DataFimExclusiva) CalcularIntervalo(
		PeriodoRelatorio periodo,
		DateTime dataReferencia)
	{
		var dataBase = DateTime.SpecifyKind(dataReferencia.Date, DateTimeKind.Utc);

		return periodo switch
		{
			PeriodoRelatorio.Semana => CalcularSemana(dataBase),
			PeriodoRelatorio.Mes => CalcularMes(dataBase),
			PeriodoRelatorio.Ano => CalcularAno(dataBase),
			_ => throw new ArgumentOutOfRangeException(nameof(periodo), periodo, "Período de relatório inválido.")
		};
	}

	private static (DateTime DataInicio, DateTime DataFimExclusiva) CalcularSemana(DateTime dataBase)
	{
		var diasDesdeSegunda = ((int)dataBase.DayOfWeek + 6) % 7;
		var inicio = dataBase.AddDays(-diasDesdeSegunda);
		return (inicio, inicio.AddDays(7));
	}

	private static (DateTime DataInicio, DateTime DataFimExclusiva) CalcularMes(DateTime dataBase)
	{
		var inicio = new DateTime(dataBase.Year, dataBase.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		return (inicio, inicio.AddMonths(1));
	}

	private static (DateTime DataInicio, DateTime DataFimExclusiva) CalcularAno(DateTime dataBase)
	{
		var inicio = new DateTime(dataBase.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (inicio, inicio.AddYears(1));
	}
}

