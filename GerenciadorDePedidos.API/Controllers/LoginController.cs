using GerenciadorDePedidos.Application.Commands.LoginCliente;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoginController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginClienteCommand command)
        {
            var loginResult = await _mediator.Send(command);
            return loginResult is null ? NotFound("Email ou senha invalidos") : Ok(loginResult);
        }
    }
}
