using ControleEstacionamento.Application.DTOs;

namespace ControleEstacionamento.Application.Interfaces;

public interface ITabelaPrecoService
{
    Task<TabelaPrecoDto> CriarAsync(TabelaPrecoCreateDto dto);
    Task<TabelaPrecoDto?> AtualizarAsync(TabelaPrecoUpdateDto dto);
    Task<bool> RemoverAsync(int id);
    Task<TabelaPrecoDto?> BuscarPorIdAsync(int id);
    Task<TabelaPrecoDto?> BuscarVigenteAsync(DateTime? data = null);
    Task<IEnumerable<TabelaPrecoDto>> ListarTodasAsync();
}
