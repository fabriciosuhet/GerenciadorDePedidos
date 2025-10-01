using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        protected readonly GerenciadorDePedidosDbContext _context;
        protected DbSet<TEntity> _dbSet;

        public Repository(GerenciadorDePedidosDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
        {
             await _dbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity is not null)
                _dbSet.Remove(entity);     
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<int> GetCountAsync()
        {
           return await _dbSet.CountAsync();
        }

        public async Task<IReadOnlyCollection<TEntity>> GetPagedAsync(int skip, int take)
        {
            return await _dbSet.Skip(skip).Take(take).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateAsync(TEntity entity)
        {
             _dbSet.Update(entity);
        }
    }
}
