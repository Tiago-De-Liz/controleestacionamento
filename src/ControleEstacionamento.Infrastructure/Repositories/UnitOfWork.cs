using ControleEstacionamento.Domain.Interfaces;
using ControleEstacionamento.Infrastructure.Data;

namespace ControleEstacionamento.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IVeiculoEstacionadoRepository? _veiculoEstacionadoRepository;
    private ITabelaPrecoRepository? _tabelaPrecoRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IVeiculoEstacionadoRepository VeiculoEstacionadoRepository =>
        _veiculoEstacionadoRepository ??= new VeiculoEstacionadoRepository(_context);

    public ITabelaPrecoRepository TabelaPrecoRepository =>
        _tabelaPrecoRepository ??= new TabelaPrecoRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
