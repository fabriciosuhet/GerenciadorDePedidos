using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
	string SenhaAtual,
	string NovaSenha,
	string ConfirmarNovaSenha) : IRequest<Unit>;

