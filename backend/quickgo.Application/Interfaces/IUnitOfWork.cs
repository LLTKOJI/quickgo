namespace quickgo.Application.Interfaces;

public interface IUnitOfWork {
    /// <summary>Guarda todo en una sola transaccion. Lanza ConflictoException si hay duplicados.</summary>
    Task GuardarAsync(CancellationToken ct = default);
}
