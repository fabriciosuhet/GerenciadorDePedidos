using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class LoginClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_CredenciaisValidas_DeveRetornarViewModelComToken()
	{
		// Arrange
		var nomeCompleto = "Nome Teste";
		var email = "email@teste.com";
		var senha = "Teste@123";
		var senhaHash = "hashSenha123";
		var telefone = "99999999999";
		var tokenFake = "token_jwt_fake";
		Role role = Role.Usuario;

		var cliente = new Cliente(nomeCompleto, email, telefone, senha, role);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.ComputeSha256Hash(senha)).Returns(senhaHash);
		authServiceMock.Setup(a => a.GenerateJwtToken(email, role.ToString())).Returns(tokenFake);
		
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetUserByEmailAndPasswordAsync(email, senhaHash)).ReturnsAsync(cliente);
		
		var handler = new LoginClienteCommandHandler(authServiceMock.Object, clienteRepositoryMock.Object);
		var command = new LoginClienteCommand{Email = email, Password = senha};
		
		// Act 
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.NotNull(result);
		Assert.Equal(email, result.Email);
		Assert.Equal(tokenFake, result.Token);

	}

	[Fact]
	public async Task Handle_CredenciaisInvalidas_DeveRetornarNull()
	{
		// Arrange
		var email = "email@teste.com";
		var senha = "Teste@123";
		var senhaHash = "hashSenha123";
		
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.ComputeSha256Hash(senha)).Returns(senhaHash);
		
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetUserByEmailAndPasswordAsync(email, senhaHash)).ReturnsAsync((Cliente)null);
		
		var handler = new LoginClienteCommandHandler(authServiceMock.Object, clienteRepositoryMock.Object);
		var command = new LoginClienteCommand {Email = email, Password = senhaHash};
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.Null(result);
	}
	
}