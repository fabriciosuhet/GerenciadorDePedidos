using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Guid>
{
	private readonly IClienteRepository _clienteRepository;
	private readonly IAuthService _authService;
	private readonly GerenciadorDePedidosDbContext _context;

	public CreateClienteCommandHandler(IClienteRepository clienteRepository, IAuthService authService, GerenciadorDePedidosDbContext context)
	{
		_clienteRepository = clienteRepository;
		_authService = authService;
		_context = context;
	}

	public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var passwordHash = _authService.ComputeSha256Hash(request.Senha);

		if (_context.Clientes.Any())
		{
			throw new Exception("O cadastro inicial já foi realizado. Use um end point autenticado");
		}
		
		var cliente = new Cliente(request.NomeCompleto, request.Email, request.Telefone, passwordHash, request.Role);
		await _clienteRepository.AddAsync(cliente);
		return cliente.Id;
	}
}