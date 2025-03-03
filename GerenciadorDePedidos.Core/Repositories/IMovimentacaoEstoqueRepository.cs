using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IMovimentacaoEstoqueRepository
{
	Task AddAsync(MovimentacaoEstoque movimentacaoEstoque);
	Task <IEnumerable<MovimentacaoEstoque>> GetAllAsync(string? query);
	Task <MovimentacaoEstoque?>GetByIdAsync(Guid id);
}