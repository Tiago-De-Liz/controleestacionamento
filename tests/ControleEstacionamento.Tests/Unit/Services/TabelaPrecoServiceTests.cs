using AutoMapper;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Mappings;
using ControleEstacionamento.Application.Services;
using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleEstacionamento.Tests.Unit.Services;

public class TabelaPrecoServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITabelaPrecoRepository> _tabelaPrecoRepoMock;
    private readonly IMapper _mapper;
    private readonly TabelaPrecoService _service;

    public TabelaPrecoServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tabelaPrecoRepoMock = new Mock<ITabelaPrecoRepository>();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = mapperConfig.CreateMapper();

        _unitOfWorkMock.Setup(u => u.TabelaPrecoRepository).Returns(_tabelaPrecoRepoMock.Object);

        _service = new TabelaPrecoService(_unitOfWorkMock.Object, _mapper);
    }

    #region CriarAsync

    [Fact]
    public async Task CriarAsync_DadosValidos_DeveCriarComSucesso()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        _tabelaPrecoRepoMock.Setup(r => r.ExistsConflitanteAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(false);
        _tabelaPrecoRepoMock.Setup(r => r.AddAsync(It.IsAny<TabelaPreco>()))
            .ReturnsAsync((TabelaPreco t) => t);

        // Act
        var result = await _service.CriarAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ValorHoraInicial.Should().Be(10m);
        result.ValorHoraAdicional.Should().Be(5m);
        _tabelaPrecoRepoMock.Verify(r => r.AddAsync(It.IsAny<TabelaPreco>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_VigenciaConflitante_DeveLancarExcecao()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        _tabelaPrecoRepoMock.Setup(r => r.ExistsConflitanteAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CriarAsync(dto));
        exception.Message.Should().Contain("conflitante");
    }

    #endregion

    #region AtualizarAsync

    [Fact]
    public async Task AtualizarAsync_TabelaExistente_DeveAtualizarComSucesso()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today.AddMonths(-1),
            DataFimVigencia = DateTime.Today,
            ValorHoraInicial = 8m,
            ValorHoraAdicional = 4m
        };

        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 12m,
            ValorHoraAdicional = 6m
        };

        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tabela);
        _tabelaPrecoRepoMock.Setup(r => r.ExistsConflitanteAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1))
            .ReturnsAsync(false);
        _tabelaPrecoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<TabelaPreco>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AtualizarAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.ValorHoraInicial.Should().Be(12m);
        result.ValorHoraAdicional.Should().Be(6m);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_TabelaNaoExistente_DeveRetornarNull()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto { Id = 999 };
        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TabelaPreco?)null);

        // Act
        var result = await _service.AtualizarAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarAsync_VigenciaConflitante_DeveLancarExcecao()
    {
        // Arrange
        var tabela = new TabelaPreco { Id = 1 };
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1)
        };

        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tabela);
        _tabelaPrecoRepoMock.Setup(r => r.ExistsConflitanteAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AtualizarAsync(dto));
    }

    #endregion

    #region RemoverAsync

    [Fact]
    public async Task RemoverAsync_TabelaExistente_DeveRemoverComSucesso()
    {
        // Arrange
        var tabela = new TabelaPreco { Id = 1 };
        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tabela);
        _tabelaPrecoRepoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoverAsync(1);

        // Assert
        result.Should().BeTrue();
        _tabelaPrecoRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_TabelaNaoExistente_DeveRetornarFalse()
    {
        // Arrange
        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TabelaPreco?)null);

        // Act
        var result = await _service.RemoverAsync(999);

        // Assert
        result.Should().BeFalse();
        _tabelaPrecoRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Buscas

    [Fact]
    public async Task BuscarPorIdAsync_TabelaExistente_DeveRetornarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            Id = 1,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1)
        };
        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tabela);

        // Act
        var result = await _service.BuscarPorIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.ValorHoraInicial.Should().Be(10m);
    }

    [Fact]
    public async Task BuscarPorIdAsync_TabelaNaoExistente_DeveRetornarNull()
    {
        // Arrange
        _tabelaPrecoRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TabelaPreco?)null);

        // Act
        var result = await _service.BuscarPorIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BuscarVigenteAsync_ComTabelaVigente_DeveRetornarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            Id = 1,
            ValorHoraInicial = 10m,
            DataInicioVigencia = DateTime.Today.AddDays(-10),
            DataFimVigencia = DateTime.Today.AddDays(10)
        };
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(It.IsAny<DateTime>())).ReturnsAsync(tabela);

        // Act
        var result = await _service.BuscarVigenteAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task BuscarVigenteAsync_SemTabelaVigente_DeveRetornarNull()
    {
        // Arrange
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((TabelaPreco?)null);

        // Act
        var result = await _service.BuscarVigenteAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BuscarVigenteAsync_ComDataEspecifica_DeveUsarDataInformada()
    {
        // Arrange
        var dataEspecifica = new DateTime(2025, 6, 15);
        _tabelaPrecoRepoMock.Setup(r => r.GetVigenteAsync(dataEspecifica))
            .ReturnsAsync(new TabelaPreco { Id = 1 });

        // Act
        var result = await _service.BuscarVigenteAsync(dataEspecifica);

        // Assert
        result.Should().NotBeNull();
        _tabelaPrecoRepoMock.Verify(r => r.GetVigenteAsync(dataEspecifica), Times.Once);
    }

    [Fact]
    public async Task ListarTodasAsync_DeveRetornarTodasAsTabelas()
    {
        // Arrange
        var tabelas = new List<TabelaPreco>
        {
            new() { Id = 1, ValorHoraInicial = 10m, DataInicioVigencia = DateTime.Today, DataFimVigencia = DateTime.Today.AddMonths(1) },
            new() { Id = 2, ValorHoraInicial = 12m, DataInicioVigencia = DateTime.Today.AddMonths(1), DataFimVigencia = DateTime.Today.AddMonths(2) }
        };
        _tabelaPrecoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tabelas);

        // Act
        var result = await _service.ListarTodasAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion
}
