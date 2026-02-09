namespace ControleEstacionamento.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVeiculoEstacionadoRepository VeiculoEstacionadoRepository { get; }
    ITabelaPrecoRepository TabelaPrecoRepository { get; }
    Task<int> SaveChangesAsync();
}
