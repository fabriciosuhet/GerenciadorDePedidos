using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public class DeleteProdutoCommandHandler : IRequestHandler<DeleteProdutoCommand, Unit>
{
	private readonly IRepository<Produto, int> _produtoRepository;

    public DeleteProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<Unit> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = await _produtoRepository.GetByIdAsync(request.Id);
		if (produto is null)
			throw new KeyNotFoundException("Produto nao encontrado");
		
		await _produtoRepository.DeleteAsync(produto.Id);
		await _produtoRepository.SaveChangesAsync();
		return Unit.Value;
	}
}