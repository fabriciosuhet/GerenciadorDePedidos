namespace GerenciadorDePedidos.Core.Entities;

public class Cliente : BaseEntity
{
	public string NomeCompleto { get; private set; }
	public string Email { get; private set; }
	public string Telefone { get; private set; }
	
	public Guid PedidoId { get; private set; }
	public List<Pedido> Pedidos  { get; private set; }

	public Cliente()
	{
		
	}
	

	public Cliente(string nomeCompleto, string email, string telefone)
	{
		NomeCompleto = nomeCompleto;
		Email = email;
		Telefone = telefone;
		
		Pedidos = new List<Pedido>();
	}
}