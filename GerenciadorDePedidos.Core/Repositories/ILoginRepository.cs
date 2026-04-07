using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories
{
    public interface ILoginRepository
    {
        Task<Login?> GetEmailAndPasswordAsync(string email, string password);
    }
}
