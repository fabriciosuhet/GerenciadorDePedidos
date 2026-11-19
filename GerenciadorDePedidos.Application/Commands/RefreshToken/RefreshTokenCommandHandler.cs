using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
	IAuthService authService,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, LoginClienteViewModel>
{
	public async Task<LoginClienteViewModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		var existingToken = await refreshTokenRepository.GetByTokenWithLoginAsync(request.RefreshToken, cancellationToken)
			?? throw new UnauthorizedAccessException("Refresh token inválido.");

		// Detecção de reuso: se um token já revogado for reapresentado, revoga todas as sessões ativas do usuário
		if (existingToken.IsRevoked)
		{
			await refreshTokenRepository.RevokeAllActiveByLoginIdAsync(existingToken.LoginId, cancellationToken);
			await unitOfWork.SaveChangesAsync(cancellationToken);
			throw new UnauthorizedAccessException("Refresh token inválido ou reutilizado.");
		}

		if (existingToken.IsExpired)
			throw new UnauthorizedAccessException("Refresh token expirado.");

		var newAccessToken = authService.GenerateJwtToken(existingToken.Login.Email, existingToken.Login.Role.ToString());
		var newRefreshToken = authService.GenerateRefreshToken();

		existingToken.Revogar(newRefreshToken);

		await refreshTokenRepository.AddAsync(
			new Core.Entities.RefreshToken(existingToken.LoginId, newRefreshToken, authService.GetRefreshTokenExpirationUtc()),
			cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new LoginClienteViewModel(existingToken.Login.Email, newAccessToken, newRefreshToken);
	}
}