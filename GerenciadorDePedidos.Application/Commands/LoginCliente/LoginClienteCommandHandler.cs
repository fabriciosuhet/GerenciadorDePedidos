using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public class LoginClienteCommandHandler : IRequestHandler<LoginClienteCommand, LoginClienteViewModel>
{
	private readonly IAuthService _authService;
	private readonly IClienteRepository _clienteRepository;

	public LoginClienteCommandHandler(IAuthService authService, IClienteRepository clienteRepository)
	{
		_authService = authService;
		_clienteRepository = clienteRepository;
	}

	public async Task<LoginClienteViewModel> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
	{
		// utilizar o mesmo algoritmo para criar o hash dessa senha
		var passwordHash = _authService.ComputeSha256Hash(request.Password);
		
		// buscar no meu banco de dados um cliente que tenha meu email e minha senha em formato de hash
		var cliente = await _clienteRepository.GetUserByEmailAndPasswordAsync(request.Email, passwordHash);
		
		// se nao existir, erro de login
		if (cliente == null) return null;

		// se existir, gero o token usando dados do usuário
		var token = _authService.GenerateJwtToken(cliente.Email, cliente.Role.ToString());
		return new LoginClienteViewModel(cliente.Email, token);
	}
}