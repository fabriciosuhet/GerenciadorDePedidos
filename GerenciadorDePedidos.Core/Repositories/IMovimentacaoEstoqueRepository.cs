using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IMovimentacaoEstoqueRepository : IRepository<MovimentacaoEstoque, int>
{
    Task<MovimentacaoEstoque?>? GetByIdMovimentacaoAsync(int id);
    Task<IEnumerable<MovimentacaoEstoque?>>? GetAllMovimentacaoAsync(string? query);
}