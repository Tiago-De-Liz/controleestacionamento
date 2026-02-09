using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;
using ControleEstacionamento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleEstacionamento.Infrastructure.Repositories;

public class VeiculoEstacionadoRepository : IVeiculoEstacionadoRepository
{
    private readonly AppDbContext _context;

    public VeiculoEstacionadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VeiculoEstacionado?> GetByIdAsync(int id)
    {
        return await _context.VeiculosEstacionados.FindAsync(id);
    }

    public async Task<VeiculoEstacionado?> GetByPlacaAtualAsync(string placa)
    {
        return await _context.VeiculosEstacionados
            .Where(v => v.Placa == placa && v.DataHoraSaida == null)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<VeiculoEstacionado>> GetAllAsync()
    {
        return await _context.VeiculosEstacionados
            .OrderByDescending(v => v.DataHoraEntrada)
            .ToListAsync();
    }

    public async Task<IEnumerable<VeiculoEstacionado>> GetVeiculosEstacionadosAsync()
    {
        return await _context.VeiculosEstacionados
            .Where(v => v.DataHoraSaida == null)
            .OrderBy(v => v.DataHoraEntrada)
            .ToListAsync();
    }

    public async Task<VeiculoEstacionado> AddAsync(VeiculoEstacionado veiculo)
    {
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        return veiculo;
    }

    public Task UpdateAsync(VeiculoEstacionado veiculo)
    {
        _context.VeiculosEstacionados.Update(veiculo);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsVeiculoEstacionadoAsync(string placa)
    {
        return await _context.VeiculosEstacionados
            .AnyAsync(v => v.Placa == placa && v.DataHoraSaida == null);
    }
}
