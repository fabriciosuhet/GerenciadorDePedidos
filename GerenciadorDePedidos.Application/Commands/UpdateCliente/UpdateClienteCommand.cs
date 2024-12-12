using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public class UpdateClienteCommand : IRequest<Unit>

{
	public Guid Id { get; set; }
	public string Email { get; set; }
	public string Telefone { get; set; }
}