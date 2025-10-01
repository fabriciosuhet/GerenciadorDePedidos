using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public class MovimentacaoEstoqueResponseDTO
{
	public int Id { get;  set; }
	public int ProdutoId { get; set; }
	public string ProdutoNome { get; set; }

}