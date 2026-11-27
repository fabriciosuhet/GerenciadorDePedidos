namespace GerenciadorDePedidos.Core.DTOs;

public sealed record ArquivoRelatorioDTO(
	byte[] Conteudo,
	string ContentType,
	string NomeArquivo);

