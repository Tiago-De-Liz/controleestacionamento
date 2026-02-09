using ControleEstacionamento.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace ControleEstacionamento.Tests.Unit.DTOs;

public class VeiculoResponseDtoTests
{
    [Fact]
    public void TempoEstadia_ComSaida_MenosDeUmaHora_DeveFormatarMinutos()
    {
        // Arrange
        var dto = new VeiculoResponseDto
        {
            DataHoraEntrada = DateTime.Now.AddMinutes(-30),
            DataHoraSaida = DateTime.Now
        };

        // Act
        var result = dto.TempoEstadia;

        // Assert
        result.Should().Contain("min");
        result.Should().NotContain("h");
    }

    [Fact]
    public void TempoEstadia_ComSaida_MaisDeUmaHora_DeveFormatarHorasEMinutos()
    {
        // Arrange
        var dto = new VeiculoResponseDto
        {
            DataHoraEntrada = DateTime.Now.AddHours(-2).AddMinutes(-15),
            DataHoraSaida = DateTime.Now
        };

        // Act
        var result = dto.TempoEstadia;

        // Assert
        result.Should().Contain("h");
        result.Should().Contain("min");
    }

    [Fact]
    public void TempoEstadia_SemSaida_DeveCalcularAteAgora()
    {
        // Arrange
        var dto = new VeiculoResponseDto
        {
            DataHoraEntrada = DateTime.Now.AddMinutes(-45),
            DataHoraSaida = null
        };

        // Act
        var result = dto.TempoEstadia;

        // Assert
        result.Should().Contain("min");
    }

    [Fact]
    public void TempoEstadia_SemSaida_MaisDeUmaHora_DeveFormatarHorasEMinutos()
    {
        // Arrange
        var dto = new VeiculoResponseDto
        {
            DataHoraEntrada = DateTime.Now.AddHours(-3).AddMinutes(-20),
            DataHoraSaida = null
        };

        // Act
        var result = dto.TempoEstadia;

        // Assert
        result.Should().Contain("h");
        result.Should().Contain("min");
    }
}

public class PaginatedListTests
{
    [Fact]
    public void Create_ComItens_DevePaginarCorretamente()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.PageIndex.Should().Be(1);
        result.TotalCount.Should().Be(100);
        result.TotalPages.Should().Be(10);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void Create_SegundaPagina_DeveRetornarItensCorretos()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 2, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.Items.First().Should().Be(11);
        result.Items.Last().Should().Be(20);
    }

    [Fact]
    public void Create_UltimaPaginaParcial_DeveRetornarItensRestantes()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 3, 10);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasPreviousPage_PrimeiraPagina_DeveRetornarFalse()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 1, 10);

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_SegundaPagina_DeveRetornarTrue()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 2, 10);

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_UltimaPagina_DeveRetornarFalse()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 10, 10);

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_PrimeiraPagina_DeveRetornarTrue()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, 1, 10);

        // Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Constructor_DeveCriarInstanciaCorretamente()
    {
        // Arrange
        var items = new List<string> { "A", "B", "C" };

        // Act
        var result = new PaginatedList<string>(items, 30, 2, 10);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(30);
        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void Create_ListaVazia_DeveRetornarListaVazia()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var result = PaginatedList<int>.Create(items, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
