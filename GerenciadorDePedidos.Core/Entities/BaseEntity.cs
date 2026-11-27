namespace GerenciadorDePedidos.Core.Entities;

public abstract class BaseEntity<TKey>
{
	public TKey Id { get; set; } = default!;
	public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
	public DateTime? AtualizadoEm { get; protected set; }
	public bool Ativo { get; protected set; } = true;
	public DateTime? DesativadoEm { get; protected set; }

	public void MarcarComoAtualizado(DateTime? dataUtc = null)
	{
		AtualizadoEm = dataUtc ?? DateTime.UtcNow;
	}

	public void Desativar(DateTime? dataUtc = null)
	{
		if (!Ativo)
			return;

		var agora = dataUtc ?? DateTime.UtcNow;
		Ativo = false;
		DesativadoEm = agora;
		AtualizadoEm = agora;
	}

	public void Reativar(DateTime? dataUtc = null)
	{
		if (Ativo)
			return;

		Ativo = true;
		DesativadoEm = null;
		AtualizadoEm = dataUtc ?? DateTime.UtcNow;
	}
}