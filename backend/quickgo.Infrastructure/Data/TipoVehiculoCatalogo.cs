using quickgo.Domain.entities;

namespace quickgo.Infrastructure.Data;

public static class TipoVehiculoCatalogo
{
    public static IReadOnlyList<TipoVehiculo> DatosIniciales { get; } =
    [
        new()
        {
            TipoVehiculoId = Guid.Parse("a23fa570-2f4b-4f85-80e9-042d0d315001"),
            Nombre = "Camión de carga",
            Cantidadkilogramo = 12000,
            LargoMetros = 8,
            AnchoMetros = 2.5m,
            AlturaMetros = 3,
            VolumenMaximoMetrosCubicos = 60
        },
        new()
        {
            TipoVehiculoId = Guid.Parse("a23fa570-2f4b-4f85-80e9-042d0d315002"),
            Nombre = "Plataforma",
            Cantidadkilogramo = 15000,
            LargoMetros = 10,
            AnchoMetros = 2.5m,
            AlturaMetros = 1,
            VolumenMaximoMetrosCubicos = 25
        },
        new()
        {
            TipoVehiculoId = Guid.Parse("a23fa570-2f4b-4f85-80e9-042d0d315003"),
            Nombre = "Montacargas",
            Cantidadkilogramo = 3000,
            LargoMetros = 2.5m,
            AnchoMetros = 1.2m,
            AlturaMetros = 2.2m,
            VolumenMaximoMetrosCubicos = 6.6m
        },
        new()
        {
            TipoVehiculoId = Guid.Parse("a23fa570-2f4b-4f85-80e9-042d0d315004"),
            Nombre = "Excavadora",
            Cantidadkilogramo = 20000,
            LargoMetros = 7,
            AnchoMetros = 2.5m,
            AlturaMetros = 3.2m,
            VolumenMaximoMetrosCubicos = 56
        }
    ];
}
