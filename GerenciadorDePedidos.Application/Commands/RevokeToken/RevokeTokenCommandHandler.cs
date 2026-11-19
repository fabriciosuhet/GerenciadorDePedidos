using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler(
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<RevokeTokenCommand, Unit>
{
	public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
	{
		var existingToken = await refreshTokenRepository.GetByTokenWithLoginAsync(request.RefreshToken, cancellationToken)
			?? throw new KeyNotFoundException("Refresh token não encontrado.");

		existingToken.Revogar();
		await unitOfWork.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}