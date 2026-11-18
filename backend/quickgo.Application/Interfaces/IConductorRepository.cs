using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Interfaces;

public interface IConductorRepository {
    Task<Conductor?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Conductor>> ListarAsync(EstadoConductor? estado, TipoConductor? tipo, Guid? soloId);
    Task<bool> ExisteLicenciaAsync(string licencia, Guid? excluirId);
    Task<IReadOnlyList<Conductor>> ListarConLicenciaHastaAsync(DateOnly limite);
    void Add(Conductor conductor);
}
