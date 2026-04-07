using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories
{
    public class LoginRepository : Repository<Login, int>, ILoginRepository
    {
        public LoginRepository(GerenciadorDePedidosDbContext context) : base(context)
        {
        }

        public async Task<Login?> GetEmailAndPasswordAsync(string email, string password)
        {
            return await _dbSet.AsNoTracking()
                .Include(l => l.Cliente)
                .SingleOrDefaultAsync(l => l.Email.Equals(email) && l.SenhaHash.Equals(password));
        }
    }
}
