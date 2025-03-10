using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public class MovimentacaoEstoqueDetailsDTO
{
	public Guid Id { get;  set; }
	public Guid ProdutoId { get; set; }
	public string ProdutoNome { get; set; }
	public Cliente Nome { get; set; }
	public string ClienteNome { get; set; }
	public int Quantidade { get; set; }
	public Tipo TipoMovimentacao { get; set; }
	public DateTime Data { get; set; }
}