using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IMovimentacaoEstoqueRepository
{
    Task<MovimentacaoEstoque?>? GetByIdMovimentacaoAsync(int id);
    Task<IEnumerable<MovimentacaoEstoque?>>? GetAllMovimentacaoAsync(string? query);
}