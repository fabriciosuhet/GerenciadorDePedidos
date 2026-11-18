using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RegisterCliente;

public sealed class RegisterClienteCommandHandler(ISender sender)
	: IRequestHandler<RegisterClienteCommand, CreateClienteViewModel>
{
	public Task<CreateClienteViewModel> Handle(RegisterClienteCommand request, CancellationToken cancellationToken)
		=> sender.Send(
			new CreateClienteCommand(request.NomeCompleto, Role.Usuario, request.Senha, request.Email, request.Telefone),
			cancellationToken);
}
