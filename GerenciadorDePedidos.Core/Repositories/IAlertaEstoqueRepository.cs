using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IAlertaEstoqueRepository : IRepository<AlertaEstoque, int>
{
	Task<AlertaEstoque?> GetPendenteByProdutoIdAsync(int produtoId, CancellationToken cancellationToken = default);
	Task<List<AlertaEstoque>> GetAllAsync(bool? apenasPendentes = null, CancellationToken cancellationToken = default);
}

