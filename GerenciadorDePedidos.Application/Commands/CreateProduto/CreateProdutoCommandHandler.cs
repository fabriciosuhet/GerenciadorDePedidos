using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, int>
{
	private readonly IRepository<Produto, int> _produtoRepository;

    public CreateProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<int> Handle(CreateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = new Produto(request.Nome, request.Preco, request.Estoque);
		await _produtoRepository.AddAsync(produto);

		await _produtoRepository.SaveChangesAsync();
        return produto.Id;
	}
}