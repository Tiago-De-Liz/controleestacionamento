using ControleEstacionamento.Domain.Entities;

namespace ControleEstacionamento.Domain.Interfaces;

public interface ITabelaPrecoRepository
{
    Task<TabelaPreco?> GetByIdAsync(int id);
    Task<TabelaPreco?> GetVigenteAsync(DateTime data);
    Task<IEnumerable<TabelaPreco>> GetAllAsync();
    Task<TabelaPreco> AddAsync(TabelaPreco tabelaPreco);
    Task UpdateAsync(TabelaPreco tabelaPreco);
    Task DeleteAsync(int id);
    Task<bool> ExistsConflitanteAsync(DateTime inicio, DateTime fim, int? excludeId = null);
}
