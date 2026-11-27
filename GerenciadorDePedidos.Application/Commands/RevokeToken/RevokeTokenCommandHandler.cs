using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler(
	IAuthService authService,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork,
	ICurrentUserService currentUserService) : IRequestHandler<RevokeTokenCommand, Unit>
{
	public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
	{
		var tokenHash = authService.HashToken(request.RefreshToken);
		var existingToken = await refreshTokenRepository.GetByTokenWithLoginAsync(tokenHash, cancellationToken)
			?? throw new KeyNotFoundException("Refresh token não encontrado.");

		if (!currentUserService.IsAdmin)
		{
			if (currentUserService.ClienteId is null || existingToken.Login is null || existingToken.Login.ClienteId != currentUserService.ClienteId.Value)
				throw new UnauthorizedAccessException("Você não tem permissão para revogar este token.");
		}

		existingToken.Revogar();
		await unitOfWork.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}