using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public class CreateClienteCommand : IRequest<Guid>
{
	public string NomeCompleto { get; set; }
	public string Email { get; set; }
	public string Telefone { get; set; }
}