using quickgo.Application.Interfaces;
using quickgo.Infrastructure.Data;
using quickgo.Infrastructure.Repositories;

namespace quickgo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) {
        
        // 1. Configuramos el acceso a PostgreSQL
        services.AddDbContext<QuickGoDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddScoped<ITipoVehiculoRepository, TipoVehiculoRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();

        return services;
    }
}
