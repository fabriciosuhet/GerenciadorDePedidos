using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class Repository<TEntity, TKey>(GerenciadorDePedidosDbContext context)
	: IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
	protected readonly GerenciadorDePedidosDbContext _context = context;
	protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

	public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		=> await _dbSet.AddAsync(entity, cancellationToken);

	public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
	{
		var entity = await GetByIdAsync(id, cancellationToken);
		if (entity is not null)
			_dbSet.Remove(entity);
	}

	public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
		=> await _dbSet.FindAsync([id], cancellationToken);

	public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
		=> await _dbSet.AsNoTracking().CountAsync(cancellationToken);

	public async Task<IReadOnlyCollection<TEntity>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
		=> await _dbSet.AsNoTracking().Skip(skip).Take(take).ToListAsync(cancellationToken);

	public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		=> await _context.SaveChangesAsync(cancellationToken);

	public void Update(TEntity entity) => _dbSet.Update(entity);
}
