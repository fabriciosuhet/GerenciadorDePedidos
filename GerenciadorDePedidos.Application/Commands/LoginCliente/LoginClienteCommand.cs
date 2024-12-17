using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public class LoginClienteCommand : IRequest<LoginClienteViewModel>
{
	public string Email { get; set; }
	public string Password { get; set; }
}