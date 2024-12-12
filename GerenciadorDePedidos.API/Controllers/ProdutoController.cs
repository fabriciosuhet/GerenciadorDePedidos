using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/produto")]
public class ProdutoController : ControllerBase
{
	[HttpGet]
	public IActionResult GetAll(string? query)
	{
		return Ok();
	}

	[HttpGet("{id}")]
	public IActionResult GetById(Guid id)
	{
		// NotFound();
		return Ok();
	}

	[HttpPost]
	public IActionResult Post([FromBody] CriarProdutoDTO criarProduto)
	{
		return CreatedAtAction(nameof(GetById), new { id = criarProduto }, criarProduto);
	}

	[HttpPut("atualizar-produto/{id}")]
	public IActionResult Put(Guid id, Produto produto)
	{
		return NoContent();
	}

	[HttpDelete("remover-produto/{id}")]
	public IActionResult Delete(Guid id)
	{
		return Ok("Produto deletado com sucesso.");
	}
}