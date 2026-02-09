using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Infrastructure.Data;
using ControleEstacionamento.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ControleEstacionamento.Tests.Integration.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    #region Repositories

    [Fact]
    public void VeiculoEstacionadoRepository_DeveRetornarInstancia()
    {
        // Act
        var repository = _unitOfWork.VeiculoEstacionadoRepository;

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void VeiculoEstacionadoRepository_DeveRetornarMesmaInstancia()
    {
        // Act
        var repository1 = _unitOfWork.VeiculoEstacionadoRepository;
        var repository2 = _unitOfWork.VeiculoEstacionadoRepository;

        // Assert
        repository1.Should().BeSameAs(repository2);
    }

    [Fact]
    public void TabelaPrecoRepository_DeveRetornarInstancia()
    {
        // Act
        var repository = _unitOfWork.TabelaPrecoRepository;

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void TabelaPrecoRepository_DeveRetornarMesmaInstancia()
    {
        // Act
        var repository1 = _unitOfWork.TabelaPrecoRepository;
        var repository2 = _unitOfWork.TabelaPrecoRepository;

        // Assert
        repository1.Should().BeSameAs(repository2);
    }

    #endregion

    #region SaveChangesAsync

    [Fact]
    public async Task SaveChangesAsync_ComAlteracoes_DeveSalvarComSucesso()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now
        };
        await _unitOfWork.VeiculoEstacionadoRepository.AddAsync(veiculo);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        var veiculoSalvo = await _context.VeiculosEstacionados.FirstOrDefaultAsync();
        veiculoSalvo.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_SemAlteracoes_DeveRetornarZero()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_MultiplaEntidades_DeveSalvarTodas()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now
        };
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        await _unitOfWork.VeiculoEstacionadoRepository.AddAsync(veiculo);
        await _unitOfWork.TabelaPrecoRepository.AddAsync(tabela);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(2);
        (await _context.VeiculosEstacionados.CountAsync()).Should().Be(1);
        (await _context.TabelasPreco.CountAsync()).Should().Be(1);
    }

    #endregion

    #region Transações

    [Fact]
    public async Task UnitOfWork_TransacaoCompleta_DeveManterConsistencia()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "XYZ9A87",
            DataHoraEntrada = DateTime.Now
        };

        // Act
        await _unitOfWork.VeiculoEstacionadoRepository.AddAsync(veiculo);
        await _unitOfWork.SaveChangesAsync();

        veiculo.DataHoraSaida = DateTime.Now;
        veiculo.ValorCobrado = 10m;
        await _unitOfWork.VeiculoEstacionadoRepository.UpdateAsync(veiculo);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var veiculoFinal = await _context.VeiculosEstacionados.FindAsync(veiculo.Id);
        veiculoFinal.Should().NotBeNull();
        veiculoFinal!.DataHoraSaida.Should().NotBeNull();
        veiculoFinal.ValorCobrado.Should().Be(10m);
    }

    #endregion
}
