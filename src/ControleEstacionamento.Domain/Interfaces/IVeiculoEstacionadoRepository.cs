using ControleEstacionamento.Domain.Entities;

namespace ControleEstacionamento.Domain.Interfaces;

public interface IVeiculoEstacionadoRepository
{
    Task<VeiculoEstacionado?> GetByIdAsync(int id);
    Task<VeiculoEstacionado?> GetByPlacaAtualAsync(string placa);
    Task<IEnumerable<VeiculoEstacionado>> GetAllAsync();
    Task<IEnumerable<VeiculoEstacionado>> GetVeiculosEstacionadosAsync();
    Task<VeiculoEstacionado> AddAsync(VeiculoEstacionado veiculo);
    Task UpdateAsync(VeiculoEstacionado veiculo);
    Task<bool> ExistsVeiculoEstacionadoAsync(string placa);
}
