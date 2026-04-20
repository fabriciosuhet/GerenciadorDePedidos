using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Guid>
{
	private readonly IAuthService _authService;
	private readonly IClienteRepository _clienteRepository;
	private readonly ILoginRepository _loginRepository;
    private readonly GerenciadorDePedidosDbContext _context;

    public CreateClienteCommandHandler(IAuthService authService, IClienteRepository clienteRepository, GerenciadorDePedidosDbContext context, ILoginRepository loginRepository)
    {
        _authService = authService;
        _clienteRepository = clienteRepository;
		_loginRepository = loginRepository;
        _context = context;
    }

    public async Task<Guid> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var passwordHash = _authService.ComputeSha256Hash(request.Senha);

        var cliente = new Cliente(request.NomeCompleto, request.Telefone, request.Role);

        var login = new Login(cliente.Id, request.Email, passwordHash, request.Role);

        await _clienteRepository.AddAsync(cliente);
        await _loginRepository.AddAsync(login);

        await _context.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }
}