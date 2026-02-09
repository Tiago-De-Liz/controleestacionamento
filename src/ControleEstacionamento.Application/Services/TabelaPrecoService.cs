using AutoMapper;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Interfaces;
using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;

namespace ControleEstacionamento.Application.Services;

public class TabelaPrecoService : ITabelaPrecoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TabelaPrecoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TabelaPrecoDto> CriarAsync(TabelaPrecoCreateDto dto)
    {
        if (await _unitOfWork.TabelaPrecoRepository.ExistsConflitanteAsync(dto.DataInicioVigencia, dto.DataFimVigencia))
        {
            throw new InvalidOperationException("Já existe uma tabela de preço com vigência conflitante.");
        }

        var tabela = _mapper.Map<TabelaPreco>(dto);

        await _unitOfWork.TabelaPrecoRepository.AddAsync(tabela);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TabelaPrecoDto>(tabela);
    }

    public async Task<TabelaPrecoDto?> AtualizarAsync(TabelaPrecoUpdateDto dto)
    {
        var tabela = await _unitOfWork.TabelaPrecoRepository.GetByIdAsync(dto.Id);

        if (tabela == null)
        {
            return null;
        }

        if (await _unitOfWork.TabelaPrecoRepository.ExistsConflitanteAsync(dto.DataInicioVigencia, dto.DataFimVigencia, dto.Id))
        {
            throw new InvalidOperationException("Já existe uma tabela de preço com vigência conflitante.");
        }

        tabela.DataInicioVigencia = dto.DataInicioVigencia;
        tabela.DataFimVigencia = dto.DataFimVigencia;
        tabela.ValorHoraInicial = dto.ValorHoraInicial;
        tabela.ValorHoraAdicional = dto.ValorHoraAdicional;

        await _unitOfWork.TabelaPrecoRepository.UpdateAsync(tabela);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TabelaPrecoDto>(tabela);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var tabela = await _unitOfWork.TabelaPrecoRepository.GetByIdAsync(id);

        if (tabela == null)
        {
            return false;
        }

        await _unitOfWork.TabelaPrecoRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TabelaPrecoDto?> BuscarPorIdAsync(int id)
    {
        var tabela = await _unitOfWork.TabelaPrecoRepository.GetByIdAsync(id);
        return tabela == null ? null : _mapper.Map<TabelaPrecoDto>(tabela);
    }

    public async Task<TabelaPrecoDto?> BuscarVigenteAsync(DateTime? data = null)
    {
        var dataConsulta = data ?? DateTime.Now;
        var tabela = await _unitOfWork.TabelaPrecoRepository.GetVigenteAsync(dataConsulta);
        return tabela == null ? null : _mapper.Map<TabelaPrecoDto>(tabela);
    }

    public async Task<IEnumerable<TabelaPrecoDto>> ListarTodasAsync()
    {
        var tabelas = await _unitOfWork.TabelaPrecoRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TabelaPrecoDto>>(tabelas);
    }
}
