using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Services;

public interface IRelatorioVendasGenerator
{
	ArquivoRelatorioDTO Gerar(RelatorioVendasDTO relatorio, FormatoRelatorio formato);
}

