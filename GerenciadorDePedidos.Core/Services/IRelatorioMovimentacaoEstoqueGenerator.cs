using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Services;

public interface IRelatorioMovimentacaoEstoqueGenerator
{
	ArquivoRelatorioDTO Gerar(RelatorioMovimentacaoEstoqueDTO relatorio, FormatoRelatorio formato);
}

