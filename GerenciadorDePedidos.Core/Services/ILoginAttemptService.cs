namespace GerenciadorDePedidos.Core.Services;

public interface ILoginAttemptService
{
	bool IsLockedOut(string email);
	void RegisterFailedAttempt(string email);
	void ResetAttempts(string email);
}

