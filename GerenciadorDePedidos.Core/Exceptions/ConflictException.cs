namespace GerenciadorDePedidos.Core.Exceptions;

/// <summary>
/// Representa um conflito de integridade ou regra de negócio ao processar uma operação.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);

