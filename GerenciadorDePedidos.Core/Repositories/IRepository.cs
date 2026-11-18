using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
	Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
	Task<IReadOnlyCollection<TEntity>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
	Task<int> GetCountAsync(CancellationToken cancellationToken = default);
	Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
	void Update(TEntity entity);
	Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
	Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
