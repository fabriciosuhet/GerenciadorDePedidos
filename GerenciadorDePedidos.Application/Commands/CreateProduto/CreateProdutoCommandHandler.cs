using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, Guid>
{
	private readonly IProdutoRepository _produtoRepository;

	public CreateProdutoCommandHandler(IProdutoRepository produtoRepository)
	{
		_produtoRepository = produtoRepository;
	}
	public async Task<Guid> Handle(CreateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = new Produto(request.Nome, request.Preco, request.Estoque);
		await _produtoRepository.AddAsync(produto);
		return produto.Id;
	}
}