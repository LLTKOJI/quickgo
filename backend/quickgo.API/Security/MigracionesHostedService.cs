using Microsoft.EntityFrameworkCore;
using quickgo.Infrastructure.Data;

namespace quickgo.API.Security;

/// <summary>Aplica las migraciones pendientes al iniciar la API (si MigrarAlIniciar = true).</summary>
public class MigracionesHostedService : IHostedService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public MigracionesHostedService(IServiceScopeFactory scopeFactory, IConfiguration configuration) {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        if (!_configuration.GetValue<bool>("MigrarAlIniciar")) {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuickGoDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
