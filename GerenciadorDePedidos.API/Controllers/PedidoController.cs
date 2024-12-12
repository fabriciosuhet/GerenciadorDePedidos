using GerenciadorDePedidos.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/pedido")]
public class PedidoController : ControllerBase
{
	[HttpGet]
	public IActionResult GetAll(string? query)
	{
		// NotFound();
		return Ok();
	}

	[HttpGet("{id}")]
	public IActionResult GetById(Guid id)
	{
		return Ok();
	}

	[HttpPost]
	public IActionResult Post([FromBody] Pedido pedido)
	{
		return CreatedAtAction(nameof(GetById), new {id = pedido.Id}, pedido);
	}

	[HttpPut("atualizar-pedido/{id}")]
	public IActionResult Put(Guid id, [FromBody] Pedido pedido)
	{
		return Ok();
	}

	[HttpDelete("deletar-pedido/{id}")]
	public IActionResult Delete(Guid id)
	{
		return Ok("pedido deletado com sucesso");
	}
	
}