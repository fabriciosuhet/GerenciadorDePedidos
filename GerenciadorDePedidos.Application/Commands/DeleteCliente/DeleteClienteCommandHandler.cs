using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public sealed class DeleteClienteCommandHandler(
	IClienteRepository clienteRepository,
	ILoginRepository loginRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork)
	: IRequestHandler<DeleteClienteCommand, Unit>
{
	public async Task<Unit> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
	{
		var cliente = await clienteRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("O cliente nao foi encontrado");

		cliente.Desativar();
		clienteRepository.Update(cliente);

		var login = await loginRepository.GetByClienteIdAsync(cliente.Id, cancellationToken);
		if (login is not null)
		{
			login.Desativar();
			loginRepository.Update(login);
			await refreshTokenRepository.RevokeAllActiveByLoginIdAsync(login.Id, cancellationToken);
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}