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
		var tokenHash = authService.HashToken(request.RefreshToken);
		var existingToken = await refreshTokenRepository.GetByTokenWithLoginAsync(tokenHash, cancellationToken)
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

		if (!existingToken.Login.Ativo || existingToken.Login.Cliente is { Ativo: false })
		{
			await refreshTokenRepository.RevokeAllActiveByLoginIdAsync(existingToken.LoginId, cancellationToken);
			await unitOfWork.SaveChangesAsync(cancellationToken);
			throw new UnauthorizedAccessException("Conta desativada.");
		}

		var newAccessToken = authService.GenerateJwtToken(
			existingToken.Login.ClienteId,
			existingToken.Login.Email,
			existingToken.Login.Role.ToString());
		var newRefreshToken = authService.GenerateRefreshToken();
		var newRefreshTokenHash = authService.HashToken(newRefreshToken);

		existingToken.Revogar(newRefreshTokenHash);

		await refreshTokenRepository.AddAsync(
			new Core.Entities.RefreshToken(existingToken.LoginId, newRefreshTokenHash, authService.GetRefreshTokenExpirationUtc()),
			cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new LoginClienteViewModel(existingToken.Login.Email, newAccessToken, newRefreshToken);
	}
}