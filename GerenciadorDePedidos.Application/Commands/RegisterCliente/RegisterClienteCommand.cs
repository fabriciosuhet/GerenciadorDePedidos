using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RegisterCliente;

/// <summary>
/// Comando público de auto-cadastro de cliente. Não aceita Role para impedir escalonamento de privilégios.
/// </summary>
public sealed record RegisterClienteCommand(
	string NomeCompleto,
	string Senha,
	string Email,
	string Telefone) : IRequest<CreateClienteViewModel>;
