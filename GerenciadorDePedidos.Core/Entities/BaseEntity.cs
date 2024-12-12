namespace GerenciadorDePedidos.Core.Entities;

public class BaseEntity
{
	public Guid Id { get; init; } = Guid.NewGuid();
	
}