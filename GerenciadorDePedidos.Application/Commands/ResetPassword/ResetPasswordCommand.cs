using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
	string Token,
	string NovaSenha,
	string ConfirmarNovaSenha) : IRequest<Unit>;

