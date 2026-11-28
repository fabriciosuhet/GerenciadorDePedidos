using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.ChangePassword;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.ForgotPassword;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.ResetPassword;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.UnitTests.Application.Validators;

public class ValidatorsTests
{
	[Fact]
	public void CreateClienteValidator_DadosValidos_DevePassar()
	{
		var validator = new CreateClienteCommandValidator();
		var command = new CreateClienteCommand("Fabricio", Role.Admin, "Teste@123", "fabricio@teste.com", "11999999999");

		var result = validator.Validate(command);

		Assert.True(result.IsValid);
	}

	[Fact]
	public void RegisterClienteValidator_DadosInvalidos_DeveFalhar()
	{
		var validator = new RegisterClienteCommandValidator();
		var command = new RegisterClienteCommand("", "123", "email-invalido", "");

		var result = validator.Validate(command);

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterClienteCommand.Email));
		Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterClienteCommand.Senha));
	}

	[Fact]
	public void CreateClienteValidator_SenhaFracaEEmailInvalido_DeveFalhar()
	{
		var validator = new CreateClienteCommandValidator();
		var command = new CreateClienteCommand("Fabricio", Role.Usuario, "123", "email-invalido", "11999999999");

		var result = validator.Validate(command);

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateClienteCommand.Email));
		Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateClienteCommand.Senha));
	}

	[Fact]
	public void CreatePedidoValidator_ListaVazia_DeveFalhar()
	{
		var validator = new CreatePedidoCommandValidator();
		var command = new CreatePedidoCommand([], Guid.NewGuid(), Status.Pendente);

		var result = validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void CreatePedidoValidator_ItemQuantidadeZero_DeveFalhar()
	{
		var validator = new CreatePedidoCommandValidator();
		var command = new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = 1, Quantidade = 0 }], Guid.NewGuid(), Status.Pendente);

		var result = validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void CreateProdutoValidator_PrecoZero_DeveFalhar()
	{
		var validator = new CreateProdutoCommandValidator();
		var command = new CreateProdutoCommand("Produto", 0m, 10);

		var result = validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void EstoqueValidators_QuantidadeZeroOuExcessiva_DevemFalhar()
	{
		var addValidator = new AdicionarProdutoEstoqueCommandValidator();
		var remValidator = new RemoverProdutoEstoqueCommandValidator();

		Assert.False(addValidator.Validate(new AdicionarProdutoEstoqueCommand(1, 0)).IsValid);
		Assert.False(remValidator.Validate(new RemoverProdutoEstoqueCommand(1, 0)).IsValid);
		Assert.False(addValidator.Validate(new AdicionarProdutoEstoqueCommand(1, 100_001)).IsValid);
		Assert.False(remValidator.Validate(new RemoverProdutoEstoqueCommand(1, 100_001)).IsValid);
	}

	[Fact]
	public void RegisterClienteValidator_NomeComControleOuMuitoLongo_DeveFalhar()
	{
		var validator = new RegisterClienteCommandValidator();

		var comControle = validator.Validate(new RegisterClienteCommand("Joao\r\nAdmin", "User@1234", "joao@teste.com", "11999999999"));
		var muitoLongo = validator.Validate(new RegisterClienteCommand(new string('A', 151), "User@1234", "joao2@teste.com", "11999999999"));

		Assert.False(comControle.IsValid);
		Assert.False(muitoLongo.IsValid);
	}

	[Fact]
	public void CreatePedidoValidator_ItensExcessivosOuQuantidadeAcimaDoLimite_DeveFalhar()
	{
		var validator = new CreatePedidoCommandValidator();

		var muitosItens = Enumerable.Range(1, 101)
			.Select(i => new ItemPedidoDTO { ProdutoId = i, Quantidade = 1 })
			.ToList();
		Assert.False(validator.Validate(new CreatePedidoCommand(muitosItens, Guid.NewGuid(), Status.Pendente)).IsValid);

		var qtdExcessiva = new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = 1, Quantidade = 10_001 }], Guid.NewGuid(), Status.Pendente);
		Assert.False(validator.Validate(qtdExcessiva).IsValid);
	}

	[Fact]
	public void PasswordResetValidators_DadosValidosEInvalidos_DevemValidarCorretamente()
	{
		var forgotValidator = new ForgotPasswordCommandValidator();
		Assert.True(forgotValidator.Validate(new ForgotPasswordCommand("usuario@teste.com")).IsValid);
		Assert.False(forgotValidator.Validate(new ForgotPasswordCommand("email-invalido")).IsValid);

		var resetValidator = new ResetPasswordCommandValidator();
		Assert.True(resetValidator.Validate(new ResetPasswordCommand("token-valido", "Nova@1234", "Nova@1234")).IsValid);
		Assert.False(resetValidator.Validate(new ResetPasswordCommand("", "Nova@1234", "Nova@1234")).IsValid);
		Assert.False(resetValidator.Validate(new ResetPasswordCommand("token-valido", "fraca", "fraca")).IsValid);
		Assert.False(resetValidator.Validate(new ResetPasswordCommand("token-valido", "Nova@1234", "Diferente@1234")).IsValid);

		var changeValidator = new ChangePasswordCommandValidator();
		Assert.True(changeValidator.Validate(new ChangePasswordCommand("Atual@1234", "Nova@1234", "Nova@1234")).IsValid);
		Assert.False(changeValidator.Validate(new ChangePasswordCommand("", "Nova@1234", "Nova@1234")).IsValid);
		Assert.False(changeValidator.Validate(new ChangePasswordCommand("Atual@1234", "fraca", "fraca")).IsValid);
		Assert.False(changeValidator.Validate(new ChangePasswordCommand("Atual@1234", "Nova@1234", "Outra@1234")).IsValid);
	}
}
