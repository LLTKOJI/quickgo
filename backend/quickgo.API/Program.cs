using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using quickgo.Infrastructure;
using quickgo.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".env")))
{
    Env.Load();
}

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
                       ?? throw new InvalidOperationException("Falta la cadena de conexión.");

builder.Services.AddInfrastructure(connectionString);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QuickGoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    if (!await dbContext.TipoVehiculos.AnyAsync())
    {
        await dbContext.TipoVehiculos.AddRangeAsync(TipoVehiculoCatalogo.DatosIniciales.Select(tipo =>
            new quickgo.Domain.entities.TipoVehiculo
            {
                TipoVehiculoId = tipo.TipoVehiculoId,
                Nombre = tipo.Nombre,
                Cantidadkilogramo = tipo.Cantidadkilogramo,
                LargoMetros = tipo.LargoMetros,
                AnchoMetros = tipo.AnchoMetros,
                AlturaMetros = tipo.AlturaMetros,
                VolumenMaximoMetrosCubicos = tipo.VolumenMaximoMetrosCubicos
            }));
        await dbContext.SaveChangesAsync();
    }
}

app.MapGet("/health", async (QuickGoDbContext dbContext, CancellationToken cancellationToken) =>
    await dbContext.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapControllers();
app.Run();