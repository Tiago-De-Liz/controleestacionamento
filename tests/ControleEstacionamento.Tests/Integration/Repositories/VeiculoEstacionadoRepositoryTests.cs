using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Infrastructure.Data;
using ControleEstacionamento.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ControleEstacionamento.Tests.Integration.Repositories;

public class VeiculoEstacionadoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VeiculoEstacionadoRepository _repository;

    public VeiculoEstacionadoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new VeiculoEstacionadoRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_VeiculoValido_DeveAdicionarComSucesso()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1D23",
            DataHoraEntrada = DateTime.Now
        };

        // Act
        var result = await _repository.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1D23");
        var veiculoSalvo = await _context.VeiculosEstacionados.FirstOrDefaultAsync();
        veiculoSalvo.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_VeiculoExistente_DeveRetornarVeiculo()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "XYZ9A87",
            DataHoraEntrada = DateTime.Now
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(veiculo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Placa.Should().Be("XYZ9A87");
    }

    [Fact]
    public async Task GetByIdAsync_VeiculoNaoExistente_DeveRetornarNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByPlacaAtualAsync

    [Fact]
    public async Task GetByPlacaAtualAsync_VeiculoEstacionado_DeveRetornarVeiculo()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now,
            DataHoraSaida = null
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPlacaAtualAsync("ABC1234");

        // Assert
        result.Should().NotBeNull();
        result!.Placa.Should().Be("ABC1234");
    }

    [Fact]
    public async Task GetByPlacaAtualAsync_VeiculoJaSaiu_DeveRetornarNull()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now.AddHours(-2),
            DataHoraSaida = DateTime.Now
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPlacaAtualAsync("ABC1234");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPlacaAtualAsync_PlacaNaoExistente_DeveRetornarNull()
    {
        // Act
        var result = await _repository.GetByPlacaAtualAsync("ZZZ9999");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ComVeiculos_DeveRetornarTodosOrdenados()
    {
        // Arrange
        var veiculos = new List<VeiculoEstacionado>
        {
            new() { Placa = "AAA1111", DataHoraEntrada = DateTime.Now.AddHours(-3) },
            new() { Placa = "BBB2222", DataHoraEntrada = DateTime.Now.AddHours(-1) },
            new() { Placa = "CCC3333", DataHoraEntrada = DateTime.Now.AddHours(-2) }
        };
        await _context.VeiculosEstacionados.AddRangeAsync(veiculos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var lista = result.ToList();
        lista.Should().HaveCount(3);
        lista[0].Placa.Should().Be("BBB2222"); // Mais recente primeiro
    }

    [Fact]
    public async Task GetAllAsync_SemVeiculos_DeveRetornarListaVazia()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetVeiculosEstacionadosAsync

    [Fact]
    public async Task GetVeiculosEstacionadosAsync_ComVeiculosEstacionados_DeveRetornarApenasEstacionados()
    {
        // Arrange
        var veiculos = new List<VeiculoEstacionado>
        {
            new() { Placa = "AAA1111", DataHoraEntrada = DateTime.Now.AddHours(-3), DataHoraSaida = null },
            new() { Placa = "BBB2222", DataHoraEntrada = DateTime.Now.AddHours(-2), DataHoraSaida = DateTime.Now },
            new() { Placa = "CCC3333", DataHoraEntrada = DateTime.Now.AddHours(-1), DataHoraSaida = null }
        };
        await _context.VeiculosEstacionados.AddRangeAsync(veiculos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVeiculosEstacionadosAsync();

        // Assert
        var lista = result.ToList();
        lista.Should().HaveCount(2);
        lista.Should().OnlyContain(v => v.DataHoraSaida == null);
    }

    [Fact]
    public async Task GetVeiculosEstacionadosAsync_SemVeiculosEstacionados_DeveRetornarListaVazia()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "AAA1111",
            DataHoraEntrada = DateTime.Now.AddHours(-2),
            DataHoraSaida = DateTime.Now
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVeiculosEstacionadosAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_VeiculoExistente_DeveAtualizarComSucesso()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now.AddHours(-2)
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        veiculo.DataHoraSaida = DateTime.Now;
        veiculo.ValorCobrado = 15m;
        await _repository.UpdateAsync(veiculo);
        await _context.SaveChangesAsync();

        // Assert
        var veiculoAtualizado = await _context.VeiculosEstacionados.FindAsync(veiculo.Id);
        veiculoAtualizado.Should().NotBeNull();
        veiculoAtualizado!.DataHoraSaida.Should().NotBeNull();
        veiculoAtualizado.ValorCobrado.Should().Be(15m);
    }

    #endregion

    #region ExistsVeiculoEstacionadoAsync

    [Fact]
    public async Task ExistsVeiculoEstacionadoAsync_VeiculoEstacionado_DeveRetornarTrue()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now,
            DataHoraSaida = null
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsVeiculoEstacionadoAsync("ABC1234");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsVeiculoEstacionadoAsync_VeiculoJaSaiu_DeveRetornarFalse()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Placa = "ABC1234",
            DataHoraEntrada = DateTime.Now.AddHours(-2),
            DataHoraSaida = DateTime.Now
        };
        await _context.VeiculosEstacionados.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsVeiculoEstacionadoAsync("ABC1234");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsVeiculoEstacionadoAsync_PlacaNaoExistente_DeveRetornarFalse()
    {
        // Act
        var result = await _repository.ExistsVeiculoEstacionadoAsync("ZZZ9999");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
