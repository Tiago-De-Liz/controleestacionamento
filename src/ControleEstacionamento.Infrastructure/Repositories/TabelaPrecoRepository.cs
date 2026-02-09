using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;
using ControleEstacionamento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleEstacionamento.Infrastructure.Repositories;

public class TabelaPrecoRepository : ITabelaPrecoRepository
{
    private readonly AppDbContext _context;

    public TabelaPrecoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TabelaPreco?> GetByIdAsync(int id)
    {
        return await _context.TabelasPreco.FindAsync(id);
    }

    public async Task<TabelaPreco?> GetVigenteAsync(DateTime data)
    {
        return await _context.TabelasPreco
            .Where(t => t.DataInicioVigencia <= data && t.DataFimVigencia >= data)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TabelaPreco>> GetAllAsync()
    {
        return await _context.TabelasPreco
            .OrderByDescending(t => t.DataInicioVigencia)
            .ToListAsync();
    }

    public async Task<TabelaPreco> AddAsync(TabelaPreco tabelaPreco)
    {
        await _context.TabelasPreco.AddAsync(tabelaPreco);
        return tabelaPreco;
    }

    public Task UpdateAsync(TabelaPreco tabelaPreco)
    {
        _context.TabelasPreco.Update(tabelaPreco);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var tabela = await _context.TabelasPreco.FindAsync(id);
        if (tabela != null)
        {
            _context.TabelasPreco.Remove(tabela);
        }
    }

    public async Task<bool> ExistsConflitanteAsync(DateTime inicio, DateTime fim, int? excludeId = null)
    {
        return await _context.TabelasPreco
            .Where(t => excludeId == null || t.Id != excludeId)
            .AnyAsync(t =>
                (inicio >= t.DataInicioVigencia && inicio <= t.DataFimVigencia) ||
                (fim >= t.DataInicioVigencia && fim <= t.DataFimVigencia) ||
                (inicio <= t.DataInicioVigencia && fim >= t.DataFimVigencia));
    }
}
