namespace GerenciadorDePedidos.Core.Services;

public interface ICurrentUserService
{
	Guid? ClienteId { get; }
	bool IsAdmin { get; }
}
