using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public sealed class UpdateClienteCommandHandler(
	IClienteRepository clienteRepository,
	ILoginRepository loginRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<UpdateClienteCommand, Unit>
{
	public async Task<Unit> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
	{
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