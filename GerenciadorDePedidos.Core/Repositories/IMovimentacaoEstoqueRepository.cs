using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IMovimentacaoEstoqueRepository : IRepository<MovimentacaoEstoque, int>
{
	Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default);
	Task<List<MovimentacaoEstoque>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default);
	Task<MovimentacaoEstoque?> GetByIdMovimentacaoAsync(int id, CancellationToken cancellationToken = default);
}