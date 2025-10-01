using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories
{
    public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey> 
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IReadOnlyCollection<TEntity>> GetPagedAsync(int skip, int take);
        Task<int> GetCountAsync();
        Task AddAsync(TEntity entity);
        void UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
    }
}
