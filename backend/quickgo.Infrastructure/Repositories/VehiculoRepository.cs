using Microsoft.EntityFrameworkCore;
using quickgo.Application.Interfaces;
using quickgo.Domain.entities;
using quickgo.Infrastructure.Data;

namespace quickgo.Infrastructure.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly QuickGoDbContext _context;

    public VehiculoRepository(QuickGoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vehiculo>> GetAllVehiculosAsync()
    {
        return await _context.Vehiculos.ToListAsync();
    }

    public async Task<Vehiculo?> GetVehiculoByIdAsync(int id)
    {
        return await _context.Vehiculos.FindAsync(id);
    }

    public async Task<Vehiculo> AddVehiculoAsync(Vehiculo vehiculo)
    {
        //add vehiculo code
        await _context.Vehiculos.AddAsync(vehiculo);
        await _context.SaveChangesAsync();
        return vehiculo;
    }

    public async Task<Vehiculo> UpdateVehiculoAsync(Vehiculo vehiculo)
    {
        var existente = await _context.Vehiculos.FindAsync(vehiculo.VehiculoId);
        if (existente == null)
        {
            return null;
        }
        existente.VehiculoId = vehiculo.VehiculoId;
        existente.EmpresaId = vehiculo.EmpresaId;
        existente.TipoVehiculo = vehiculo.TipoVehiculo;
        existente.Vin = vehiculo.Vin;
        existente.Placa =  vehiculo.Placa;
        existente.Propiedad  = vehiculo.Propiedad;
        existente.Estado  = vehiculo.Estado;
        
        await _context.SaveChangesAsync();
        return vehiculo;
        
    }

    public async Task<Vehiculo?> DeleteVehiculoAsync(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return null;
        }
        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();
        return vehiculo;
    }
    
    
}