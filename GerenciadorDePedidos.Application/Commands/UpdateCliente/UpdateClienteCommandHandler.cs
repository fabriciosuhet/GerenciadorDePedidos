using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public sealed class UpdateClienteCommandHandler(
	IClienteRepository clienteRepository,
	ILoginRepository loginRepository,
	IUnitOfWork unitOfWork,
	ICurrentUserService currentUserService) : IRequestHandler<UpdateClienteCommand, Unit>
{
	public async Task<Unit> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
	{
		if (!currentUserService.IsAdmin && currentUserService.ClienteId != request.Id)
			throw new UnauthorizedAccessException("Você não tem permissão para atualizar os dados de outro cliente.");

		var cliente = await clienteRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Cliente nao encontrado");

		cliente.AlterarTelefone(request.Telefone);
		clienteRepository.Update(cliente);

		var login = await loginRepository.GetByClienteIdAsync(cliente.Id, cancellationToken);
		if (login is not null && !string.IsNullOrWhiteSpace(request.Email) && login.Email != request.Email)
		{
			var emailOwner = await loginRepository.GetByEmailAsync(request.Email, cancellationToken);
			if (emailOwner is not null && emailOwner.ClienteId != cliente.Id)
				throw new ArgumentException("Já existe um cliente cadastrado com este e-mail.");

			login.AlterarEmail(request.Email);
			loginRepository.Update(login);
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}