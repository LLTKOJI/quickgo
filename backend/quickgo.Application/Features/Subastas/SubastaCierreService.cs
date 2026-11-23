using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Domain.enums;

namespace quickgo.Application.Features.Subastas;

public class SubastaCierreService {
    private readonly ISubastaRepository _repository;

    public SubastaCierreService(ISubastaRepository repository) {
        _repository = repository;
    }

    // Para consultas de una sola subasta
    public async Task AplicarAsync(Subasta subasta) {
        if (subasta.Estado == EstadoSubasta.Abierta && subasta.FechaCierre <= DateTimeOffset.UtcNow) {
            await _repository.VencerAsync(new[] { subasta });
        }
    }

    // Para listados: vence todas las subastas abiertas de la empresa cuya fecha ya pasó
    public async Task AplicarEmpresaAsync(Guid empresaId) {
        var vencidas = await _repository.ObtenerAbiertasVencidasAsync(empresaId);
        await _repository.VencerAsync(vencidas);
    }

    public async Task AplicarTodasVencidasAsync() {
        var vencidas = await _repository.ObtenerAbiertasVencidasGlobalAsync();
        await _repository.VencerAsync(vencidas);
    }
}