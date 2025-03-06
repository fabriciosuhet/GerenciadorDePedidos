using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IMovimentacaoEstoqueRepository
{
	Task AddAsync(MovimentacaoEstoque movimentacaoEstoque);
	Task<int> GetCountAsync(string? query);
	Task<ICollection<MovimentacaoEstoque>> GetPagedAsync(string? query, int skip, int take);
	Task <MovimentacaoEstoque?>GetByIdAsync(Guid id);
}