using AutoMapper;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Interfaces;
using ControleEstacionamento.Application.Services.Strategies;
using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;

namespace ControleEstacionamento.Application.Services;

public class EstacionamentoService : IEstacionamentoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICalculoPrecoStrategy _calculoPrecoStrategy;

    public EstacionamentoService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICalculoPrecoStrategy calculoPrecoStrategy)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _calculoPrecoStrategy = calculoPrecoStrategy;
    }

    public async Task<VeiculoResponseDto> RegistrarEntradaAsync(VeiculoEntradaDto dto)
    {
        var placaNormalizada = dto.Placa.ToUpperInvariant().Replace("-", "");

        if (await _unitOfWork.VeiculoEstacionadoRepository.ExistsVeiculoEstacionadoAsync(placaNormalizada))
        {
            throw new InvalidOperationException($"Já existe um veículo com a placa {placaNormalizada} estacionado.");
        }

        var veiculo = new VeiculoEstacionado
        {
            Placa = placaNormalizada,
            DataHoraEntrada = DateTime.Now
        };

        await _unitOfWork.VeiculoEstacionadoRepository.AddAsync(veiculo);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<VeiculoResponseDto>(veiculo);
    }

    public async Task<VeiculoSaidaDto> RegistrarSaidaAsync(int veiculoId)
    {
        var veiculo = await _unitOfWork.VeiculoEstacionadoRepository.GetByIdAsync(veiculoId);

        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com ID {veiculoId} não encontrado.");
        }

        if (veiculo.DataHoraSaida.HasValue)
        {
            throw new InvalidOperationException("Este veículo já teve sua saída registrada.");
        }

        return await ProcessarSaida(veiculo);
    }

    public async Task<VeiculoSaidaDto> RegistrarSaidaPorPlacaAsync(string placa)
    {
        var placaNormalizada = placa.ToUpperInvariant().Replace("-", "");
        var veiculo = await _unitOfWork.VeiculoEstacionadoRepository.GetByPlacaAtualAsync(placaNormalizada);

        if (veiculo == null)
        {
            throw new KeyNotFoundException($"Veículo com placa {placaNormalizada} não encontrado no estacionamento.");
        }

        return await ProcessarSaida(veiculo);
    }

    private async Task<VeiculoSaidaDto> ProcessarSaida(VeiculoEstacionado veiculo)
    {
        var dataSaida = DateTime.Now;

        var tabelaPreco = await _unitOfWork.TabelaPrecoRepository.GetVigenteAsync(dataSaida);

        if (tabelaPreco == null)
        {
            throw new InvalidOperationException("Não há tabela de preço vigente para a data atual.");
        }

        var valor = _calculoPrecoStrategy.CalcularValor(
            veiculo.DataHoraEntrada,
            dataSaida,
            tabelaPreco.ValorHoraInicial,
            tabelaPreco.ValorHoraAdicional);

        veiculo.DataHoraSaida = dataSaida;
        veiculo.ValorCobrado = valor;

        await _unitOfWork.VeiculoEstacionadoRepository.UpdateAsync(veiculo);
        await _unitOfWork.SaveChangesAsync();

        return new VeiculoSaidaDto
        {
            Id = veiculo.Id,
            Placa = veiculo.Placa,
            DataHoraEntrada = veiculo.DataHoraEntrada,
            DataHoraSaida = dataSaida,
            TempoEstadia = dataSaida - veiculo.DataHoraEntrada,
            ValorCobrado = valor
        };
    }

    public async Task<IEnumerable<VeiculoResponseDto>> ListarVeiculosEstacionadosAsync()
    {
        var veiculos = await _unitOfWork.VeiculoEstacionadoRepository.GetVeiculosEstacionadosAsync();
        return _mapper.Map<IEnumerable<VeiculoResponseDto>>(veiculos);
    }

    public async Task<IEnumerable<VeiculoResponseDto>> ListarTodosAsync()
    {
        var veiculos = await _unitOfWork.VeiculoEstacionadoRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<VeiculoResponseDto>>(veiculos);
    }

    public async Task<VeiculoResponseDto?> BuscarPorIdAsync(int id)
    {
        var veiculo = await _unitOfWork.VeiculoEstacionadoRepository.GetByIdAsync(id);
        return veiculo == null ? null : _mapper.Map<VeiculoResponseDto>(veiculo);
    }

    public async Task<VeiculoResponseDto?> BuscarPorPlacaAsync(string placa)
    {
        var placaNormalizada = placa.ToUpperInvariant().Replace("-", "");
        var veiculo = await _unitOfWork.VeiculoEstacionadoRepository.GetByPlacaAtualAsync(placaNormalizada);
        return veiculo == null ? null : _mapper.Map<VeiculoResponseDto>(veiculo);
    }
}
