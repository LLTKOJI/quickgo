using System.Text.Json.Serialization;
using DotNetEnv;
using quickgo.API.Security;
using quickgo.Application;
using quickgo.Infrastructure;
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
                ?? throw new InvalidOperationException("Falta la cadena de conexión (ConnectionStrings__PostgresConnection).");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddQuickGoAuth(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<MigracionesHostedService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Los enums viajan como texto con el nombre exacto de cada valor
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
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
