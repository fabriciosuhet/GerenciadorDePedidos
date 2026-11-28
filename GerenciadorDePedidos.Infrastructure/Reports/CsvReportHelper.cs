using System.Globalization;
using System.Text;

namespace GerenciadorDePedidos.Infrastructure.Reports;

internal static class CsvReportHelper
{
	public const char Separator = ';';
	public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
	private static readonly TimeZoneInfo BrasiliaTimeZone = ResolveBrasiliaTimeZone();

	public static void AppendRow(StringBuilder sb, int totalColumns, params string[] cells)
	{
		for (var i = 0; i < totalColumns; i++)
		{
			if (i > 0)
				sb.Append(Separator);

			if (i < cells.Length && !string.IsNullOrEmpty(cells[i]))
				sb.Append(cells[i]);
		}

		sb.Append("\r\n");
	}

	public static void AppendBlankRow(StringBuilder sb, int totalColumns)
		=> AppendRow(sb, totalColumns);

	public static void AppendSectionHeader(StringBuilder sb, int totalColumns, string title)
		=> AppendRow(sb, totalColumns, Escape(title));

	public static string FormatCurrency(decimal valor)
	{
		var sinal = valor < 0m ? "-" : string.Empty;
		return $"{sinal}R$ {Math.Abs(valor).ToString("N2", PtBr)}";
	}

	public static string FormatPercent(decimal valor)
		=> $"{valor.ToString("N2", PtBr)}%";

	public static string FormatInteger(int valor)
		=> valor.ToString("N0", PtBr);

	public static string FormatDate(DateTime date)
		=> date.ToString("dd/MM/yyyy", PtBr);

	public static string FormatDateTimeBrasilia(DateTime utcDateTime)
	{
		var brasilia = TimeZoneInfo.ConvertTimeFromUtc(
			DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
			BrasiliaTimeZone);

		return brasilia.ToString("dd/MM/yyyy HH:mm:ss", PtBr);
	}

	public static string Escape(string? valor)
	{
		if (string.IsNullOrEmpty(valor))
			return string.Empty;

		var sanitizado = valor;
		var semEspacosIniciais = sanitizado.TrimStart();
		if ((semEspacosIniciais.Length > 0 && semEspacosIniciais[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
			|| sanitizado[0] is '\t' or '\r')
		{
			sanitizado = $"'{sanitizado}";
		}

		if (sanitizado.Contains(Separator)
			|| sanitizado.Contains('"')
			|| sanitizado.Contains('\n')
			|| sanitizado.Contains('\r')
			|| sanitizado.StartsWith('\''))
		{
			return $"\"{sanitizado.Replace("\"", "\"\"")}\"";
		}

		return sanitizado;
	}

	public static byte[] ToUtf8WithBom(string content)
	{
		var preamble = Encoding.UTF8.GetPreamble();
		var body = Encoding.UTF8.GetBytes(content);
		var result = new byte[preamble.Length + body.Length];
		Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
		Buffer.BlockCopy(body, 0, result, preamble.Length, body.Length);
		return result;
	}

	private static TimeZoneInfo ResolveBrasiliaTimeZone()
	{
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
		}
		catch (TimeZoneNotFoundException)
		{
			return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
		}
	}
}

