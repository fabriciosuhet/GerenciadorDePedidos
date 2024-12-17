using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Guid>
{
	private readonly IClienteRepository _clienteRepository;
	private readonly IAuthService _authService;

	public CreateClienteCommandHandler(IClienteRepository clienteRepository, IAuthService authService)
	{
		_clienteRepository = clienteRepository;
		_authService = authService;
	}

	public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var passwordHash = _authService.ComputeSha256Hash(request.Senha);
		
		var cliente = new Cliente(request.NomeCompleto, request.Email, request.Telefone, passwordHash, request.Role);
		await _clienteRepository.AddAsync(cliente);
		return cliente.Id;
	}
}