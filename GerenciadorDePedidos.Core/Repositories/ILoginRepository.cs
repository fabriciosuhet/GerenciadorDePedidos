using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories
{
    public interface ILoginRepository : IRepository<Login, int>
    {
        Task<Login?> GetEmailAndPasswordAsync(string email, string password);
    }
}
