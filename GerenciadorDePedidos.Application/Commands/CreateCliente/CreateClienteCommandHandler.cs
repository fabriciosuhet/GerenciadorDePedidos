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
	private readonly IAuthService _authService;
	private readonly IRepository<Cliente, Guid> _clienteRepository;
	private readonly GerenciadorDePedidosDbContext _context;

    public CreateClienteCommandHandler(IAuthService authService, IRepository<Cliente, Guid> clienteRepository, GerenciadorDePedidosDbContext context)
    {
        _authService = authService;
        _clienteRepository = clienteRepository;
        _context = context;
    }

    public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var passwordHash = _authService.ComputeSha256Hash(request.Senha);

		var cliente = new Cliente(request.NomeCompleto, request.Email, request.Telefone, passwordHash, request.Role);
		await _clienteRepository.AddAsync(cliente);
		await _context.SaveChangesAsync();
		return cliente.Id;
	}
}