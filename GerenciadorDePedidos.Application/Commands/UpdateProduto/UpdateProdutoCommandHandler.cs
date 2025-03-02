using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateProduto;

public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, Unit>
{
	private readonly IProdutoRepository _produtoRepository;

	public UpdateProdutoCommandHandler(IProdutoRepository produtoRepository)
	{
		_produtoRepository = produtoRepository;
	}

	public async Task<Unit> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto =  await _produtoRepository.GetByIdAsync(request.Id);
		produto.AlterarDados(request.Nome, request.Preco, request.Estoque);
		await _produtoRepository.UpdateAsync(request.Id, produto);
		return Unit.Value;
	}
}