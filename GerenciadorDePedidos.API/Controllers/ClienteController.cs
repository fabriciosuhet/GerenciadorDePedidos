using GerenciadorDePedidos.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/cliente")]
public class ClienteController : ControllerBase
{

	[HttpGet]
	public IActionResult GetAll(string? query)
	{
		// return NotFound();
		return Ok();
	}

	[HttpGet("{id}")]
	public IActionResult GetById(Guid id)
	{
		return Ok();
	}

	[HttpPost]
	public IActionResult Post(Cliente cliente)
	{
		return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
	}

	[HttpPut("atualizar-cliente{id}")]
	public IActionResult Put(Guid id, Cliente cliente)
	{
		return NoContent();
	}

	[HttpDelete("deletar-cliente{id}")]
	public IActionResult Delete(Guid id)
	{
		return Ok("Cliente deletado com sucesso");
	}
}