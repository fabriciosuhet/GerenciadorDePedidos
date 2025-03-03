using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public class MovimentacaoEstoqueResponseDTO
{
	public Guid Id { get;  set; }
	public Guid ProdutoId { get; set; }
	public string ProdutoNome { get; set; }

}