using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreateClienteCommandHandlerTests
{
	[Fact]
	public async Task Handler_CommandValid_ShouldCreateCliente()
	{
		// Arrange
		var command = new CreateClienteCommand
		{
			NomeCompleto = "Nome teste",
			Email = "email@teste.com",
			Telefone = "123456789",
			Senha = "Teste@123",
			Role = Role.Admin
		};
		
		// Mock de IClienteRepository
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
		
		
		// Mock de IAuthService
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.ComputeSha256Hash("Teste@123")).Returns("hashedPassword");
		
		// Mock de GerenciadorDePedidosDbContext
		var dbContextOptions = new DbContextOptionsBuilder<GerenciadorDePedidosDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		
		var dbContext = new GerenciadorDePedidosDbContext(dbContextOptions);
		
		var handler = new CreateClienteCommandHandler(clienteRepositoryMock.Object, authServiceMock.Object, dbContext);
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.NotEqual(Guid.Empty, result);
		clienteRepositoryMock.Verify(r => r.AddAsync(It.Is<Cliente>(c =>
			c.NomeCompleto.Equals("Nome teste") &&
			c.Email.Equals("email@teste.com") &&
			c.Telefone.Equals("123456789") &&
			c.Senha.Equals("hashedPassword") &&
			c.Role.Equals(Role.Admin))), Times.Once());
		authServiceMock.Verify(a => a.ComputeSha256Hash("Teste@123"), Times.Once());

	}

	[Fact]
	public async Task Handler_ClienteExisting_ThrowsException()
	{
		// Arrange
		var command = new CreateClienteCommand
		{
			NomeCompleto = "Nome Teste",
			Email = "email@teste.com",
			Telefone = "123456789",
			Role = Role.Admin,
			Senha = "Teste@123"
		};
		
		// Mock de IClienteRepository
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
		
		// Mock de IAuthService
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.ComputeSha256Hash("Teste@123")).Returns("hashedPassword");
		
		// Mock de GerenciadorDePedidosDbContext com um cliente existente
		var dbContextOptions = new DbContextOptionsBuilder<GerenciadorDePedidosDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		
		var dbContext = new GerenciadorDePedidosDbContext(dbContextOptions);
		dbContext.Clientes.Add(new Cliente("Outro Cliente", "outro@exemplo.com", "987654", "outraSenha", Role.Usuario));
		await dbContext.SaveChangesAsync();
		
		var handler = new CreateClienteCommandHandler(clienteRepositoryMock.Object, authServiceMock.Object, dbContext);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O cadastro inicial já foi realizado. Use um end point autenticado", exception.Message);
		clienteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never());
		authServiceMock.Verify(a => a.ComputeSha256Hash("Teste@123"), Times.Once());

	}
}