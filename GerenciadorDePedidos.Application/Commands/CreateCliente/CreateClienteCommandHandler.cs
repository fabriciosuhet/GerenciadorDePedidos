using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public sealed class CreateClienteCommandHandler(
	IAuthService authService,
	IClienteRepository clienteRepository,
	ILoginRepository loginRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<CreateClienteCommand, CreateClienteViewModel>
{
	public async Task<CreateClienteViewModel> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
	{
		var existingLogin = await loginRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (existingLogin is not null)
			throw new ArgumentException("Não foi possível concluir o cadastro com os dados informados.");

		var passwordHash = authService.HashPassword(request.Senha);
		var cliente = new Cliente(request.NomeCompleto, request.Telefone, request.Role);
		var login = new Login(cliente.Id, request.Email, passwordHash, request.Role);

		await clienteRepository.AddAsync(cliente, cancellationToken);
		await loginRepository.AddAsync(login, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		var accessToken = authService.GenerateJwtToken(cliente.Id, login.Email, login.Role.ToString());
		var refreshToken = authService.GenerateRefreshToken();
		var refreshTokenHash = authService.HashToken(refreshToken);

		await refreshTokenRepository.AddAsync(
			new Core.Entities.RefreshToken(login.Id, refreshTokenHash, authService.GetRefreshTokenExpirationUtc()),
			cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateClienteViewModel(cliente.Id, accessToken, refreshToken);
	}
}