using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using quickgo.Infrastructure;
using quickgo.Infrastructure.Data;
using quickgo.API.Security;
using quickgo.Application;
using Scalar.AspNetCore;

// Carga el .env (si existe) ANTES de crear el builder para que la configuración lo vea.
try
{
    Env.TraversePath().Load();
}
catch (FileNotFoundException)
{
    // Sin .env: se usan las variables de entorno del sistema.
}

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
                       ?? throw new InvalidOperationException("Falta la cadena de conexión.");



builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddQuickGoAuth(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<MigracionesHostedService>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "QuickGo API",
            Version = "v1",
            Description = "API REST de QuickGo. Autenticacion JWT Bearer. La empresa se toma del token (empresaId); no enviar empresaId en el body. Errores en formato ProblemDetails."
        };
        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Los enums viajan como texto con el nombre exacto de cada valor
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();


app.MapGet("/health", async (QuickGoDbContext dbContext, CancellationToken cancellationToken) =>
    await dbContext.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("QuickGo API")
            .WithTheme(ScalarTheme.Default)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages(); // 401, 403 y 404 también salen como ProblemDetails
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
