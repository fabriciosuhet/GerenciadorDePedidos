using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateProduto;

public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, Unit>
{
	private readonly IRepository<Produto, int> _produtoRepository;

    public UpdateProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<Unit> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto =  await _produtoRepository.GetByIdAsync(request.Id);
		produto.AlterarDados(request.Nome, request.Preco, request.Estoque);
		_produtoRepository.UpdateAsync(produto);
		await _produtoRepository.SaveChangesAsync();
		return Unit.Value;
	}
}