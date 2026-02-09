using AutoMapper;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Interfaces;
using ControleEstacionamento.Application.Mappings;
using ControleEstacionamento.Application.Services;
using ControleEstacionamento.Application.Services.Strategies;
using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleEstacionamento.Tests.Unit.Services;

public class EstacionamentoServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVeiculoEstacionadoRepository> _veiculoRepoMock;
    private readonly Mock<ITabelaPrecoRepository> _tabelaPrecoRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ICalculoPrecoStrategy> _calculoPrecoMock;
    private readonly EstacionamentoService _service;

    public EstacionamentoServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _veiculoRepoMock = new Mock<IVeiculoEstacionadoRepository>();
        _tabelaPrecoRepoMock = new Mock<ITabelaPrecoRepository>();
        _calculoPrecoMock = new Mock<ICalculoPrecoStrategy>();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = mapperConfig.CreateMapper();

        _unitOfWorkMock.Setup(u => u.VeiculoEstacionadoRepository).Returns(_veiculoRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TabelaPrecoRepository).Returns(_tabelaPrecoRepoMock.Object);

        _service = new EstacionamentoService(_unitOfWorkMock.Object, _mapper, _calculoPrecoMock.Object);
    }

    #region RegistrarEntradaAsync

    [Fact]
    public async Task RegistrarEntradaAsync_PlacaValida_DeveRegistrarComSucesso()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "ABC1D23" };
        _veiculoRepoMock.Setup(r => r.ExistsVeiculoEstacionadoAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _veiculoRepoMock.Setup(r => r.AddAsync(It.IsAny<VeiculoEstacionado>()))
            .ReturnsAsync((VeiculoEstacionado v) => v);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.RegistrarEntradaAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1D23");
        _veiculoRepoMock.Verify(r => r.AddAsync(It.IsAny<VeiculoEstacionado>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegistrarEntradaAsync_PlacaComHifen_DeveNormalizarPlaca()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "abc-1d23" };
        _veiculoRepoMock.Setup(r => r.ExistsVeiculoEstacionadoAsync("ABC1D23"))
            .ReturnsAsync(false);
        _veiculoRepoMock.Setup(r => r.AddAsync(It.IsAny<VeiculoEstacionado>()))
            .ReturnsAsync((VeiculoEstacionado v) => v);

        // Act
        var result = await _service.RegistrarEntradaAsync(dto);

        // Assert
        result.Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task RegistrarEntradaAsync_VeiculoJaEstacionado_DeveLancarExcecao()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "ABC1D23" };
        _veiculoRepoMock.Setup(r => r.ExistsVeiculoEstacionadoAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarEntradaAsync(dto));
    }

    #endregion

    #region RegistrarSaidaAsync

    [Fact]
    public async Task RegistrarSaidaAsync_VeiculoExistente_DeveRegistrarSaidaComSucesso()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Id = 1,
            Placa = "ABC1D23",
            DataHoraEntrada = DateTime.Now.AddHours(-2)
        };
        var tabelaPreco = new TabelaPreco
        {
            Id = 1,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        _veiculoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(veiculo);
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(It.IsAny<DateTime>())).ReturnsAsync(tabelaPreco);
        _calculoPrecoMock.Setup(c => c.CalcularValor(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 10m, 5m))
            .Returns(15m);
        _veiculoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<VeiculoEstacionado>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegistrarSaidaAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1D23");
        result.ValorCobrado.Should().Be(15m);
        result.DataHoraSaida.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegistrarSaidaAsync_VeiculoNaoEncontrado_DeveLancarExcecao()
    {
        // Arrange
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((VeiculoEstacionado?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RegistrarSaidaAsync(999));
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task RegistrarSaidaAsync_VeiculoJaSaiu_DeveLancarExcecao()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Id = 1,
            Placa = "ABC1D23",
            DataHoraEntrada = DateTime.Now.AddHours(-2),
            DataHoraSaida = DateTime.Now.AddHours(-1)
        };
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(veiculo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarSaidaAsync(1));
        exception.Message.Should().Contain("saída registrada");
    }

    [Fact]
    public async Task RegistrarSaidaAsync_SemTabelaPrecoVigente_DeveLancarExcecao()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Id = 1,
            Placa = "ABC1D23",
            DataHoraEntrada = DateTime.Now.AddHours(-2)
        };
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(veiculo);
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(It.IsAny<DateTime>())).ReturnsAsync((TabelaPreco?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegistrarSaidaAsync(1));
        exception.Message.Should().Contain("tabela de preço");
    }

    #endregion

    #region RegistrarSaidaPorPlacaAsync

    [Fact]
    public async Task RegistrarSaidaPorPlacaAsync_PlacaExistente_DeveRegistrarSaidaComSucesso()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado
        {
            Id = 1,
            Placa = "ABC1D23",
            DataHoraEntrada = DateTime.Now.AddHours(-1)
        };
        var tabelaPreco = new TabelaPreco
        {
            Id = 1,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        _veiculoRepoMock.Setup(r => r.GetByPlacaAtualAsync("ABC1D23")).ReturnsAsync(veiculo);
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(It.IsAny<DateTime>())).ReturnsAsync(tabelaPreco);
        _calculoPrecoMock.Setup(c => c.CalcularValor(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 10m, 5m))
            .Returns(10m);

        // Act
        var result = await _service.RegistrarSaidaPorPlacaAsync("ABC-1D23");

        // Assert
        result.Should().NotBeNull();
        result.Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task RegistrarSaidaPorPlacaAsync_PlacaNaoEncontrada_DeveLancarExcecao()
    {
        // Arrange
        _veiculoRepoMock.Setup(r => r.GetByPlacaAtualAsync(It.IsAny<string>()))
            .ReturnsAsync((VeiculoEstacionado?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RegistrarSaidaPorPlacaAsync("XYZ9999"));
    }

    #endregion

    #region Listagens

    [Fact]
    public async Task ListarVeiculosEstacionadosAsync_DeveRetornarApenasEstacionados()
    {
        // Arrange
        var veiculos = new List<VeiculoEstacionado>
        {
            new() { Id = 1, Placa = "ABC1D23", DataHoraEntrada = DateTime.Now.AddHours(-1) },
            new() { Id = 2, Placa = "XYZ9A87", DataHoraEntrada = DateTime.Now.AddHours(-2) }
        };
        _veiculoRepoMock.Setup(r => r.GetVeiculosEstacionadosAsync()).ReturnsAsync(veiculos);

        // Act
        var result = await _service.ListarVeiculosEstacionadosAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(v => v.DataHoraSaida == null).Should().BeTrue();
    }

    [Fact]
    public async Task ListarTodosAsync_DeveRetornarTodosOsVeiculos()
    {
        // Arrange
        var veiculos = new List<VeiculoEstacionado>
        {
            new() { Id = 1, Placa = "ABC1D23", DataHoraEntrada = DateTime.Now.AddHours(-5), DataHoraSaida = DateTime.Now.AddHours(-4), ValorCobrado = 10m },
            new() { Id = 2, Placa = "XYZ9A87", DataHoraEntrada = DateTime.Now.AddHours(-1) }
        };
        _veiculoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(veiculos);

        // Act
        var result = await _service.ListarTodosAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task BuscarPorIdAsync_VeiculoExistente_DeveRetornarVeiculo()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado { Id = 1, Placa = "ABC1D23", DataHoraEntrada = DateTime.Now };
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(veiculo);

        // Act
        var result = await _service.BuscarPorIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task BuscarPorIdAsync_VeiculoNaoExistente_DeveRetornarNull()
    {
        // Arrange
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((VeiculoEstacionado?)null);

        // Act
        var result = await _service.BuscarPorIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BuscarPorPlacaAsync_PlacaExistente_DeveRetornarVeiculo()
    {
        // Arrange
        var veiculo = new VeiculoEstacionado { Id = 1, Placa = "ABC1D23", DataHoraEntrada = DateTime.Now };
        _veiculoRepoMock.Setup(r => r.GetByPlacaAtualAsync("ABC1D23")).ReturnsAsync(veiculo);

        // Act
        var result = await _service.BuscarPorPlacaAsync("abc-1d23");

        // Assert
        result.Should().NotBeNull();
        result!.Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task BuscarPorPlacaAsync_PlacaNaoExistente_DeveRetornarNull()
    {
        // Arrange
        _veiculoRepoMock.Setup(r => r.GetByPlacaAtualAsync(It.IsAny<string>()))
            .ReturnsAsync((VeiculoEstacionado?)null);

        // Act
        var result = await _service.BuscarPorPlacaAsync("XYZ9999");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
