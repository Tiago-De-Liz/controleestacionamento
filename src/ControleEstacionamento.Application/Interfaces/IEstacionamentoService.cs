using ControleEstacionamento.Application.DTOs;

namespace ControleEstacionamento.Application.Interfaces;

public interface IEstacionamentoService
{
    Task<VeiculoResponseDto> RegistrarEntradaAsync(VeiculoEntradaDto dto);
    Task<VeiculoSaidaDto> RegistrarSaidaAsync(int veiculoId);
    Task<VeiculoSaidaDto> RegistrarSaidaPorPlacaAsync(string placa);
    Task<IEnumerable<VeiculoResponseDto>> ListarVeiculosEstacionadosAsync();
    Task<IEnumerable<VeiculoResponseDto>> ListarTodosAsync();
    Task<VeiculoResponseDto?> BuscarPorIdAsync(int id);
    Task<VeiculoResponseDto?> BuscarPorPlacaAsync(string placa);
}
