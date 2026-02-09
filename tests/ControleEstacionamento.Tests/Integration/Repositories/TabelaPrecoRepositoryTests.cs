using ControleEstacionamento.Domain.Entities;
using ControleEstacionamento.Infrastructure.Data;
using ControleEstacionamento.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ControleEstacionamento.Tests.Integration.Repositories;

public class TabelaPrecoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TabelaPrecoRepository _repository;

    public TabelaPrecoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TabelaPrecoRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_TabelaValida_DeveAdicionarComSucesso()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = await _repository.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.ValorHoraInicial.Should().Be(10m);
        var tabelaSalva = await _context.TabelasPreco.FirstOrDefaultAsync();
        tabelaSalva.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_TabelaExistente_DeveRetornarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tabela.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ValorHoraInicial.Should().Be(10m);
    }

    [Fact]
    public async Task GetByIdAsync_TabelaNaoExistente_DeveRetornarNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetVigenteAsync

    [Fact]
    public async Task GetVigenteAsync_ComTabelaVigente_DeveRetornarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today.AddDays(-10),
            DataFimVigencia = DateTime.Today.AddDays(10),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVigenteAsync(DateTime.Today);

        // Assert
        result.Should().NotBeNull();
        result!.ValorHoraInicial.Should().Be(10m);
    }

    [Fact]
    public async Task GetVigenteAsync_SemTabelaVigente_DeveRetornarNull()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today.AddMonths(1),
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVigenteAsync(DateTime.Today);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVigenteAsync_DataNoLimiteInicio_DeveRetornarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVigenteAsync(DateTime.Today);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetVigenteAsync_DataNoLimiteFim_DeveRetornarTabela()
    {
        // Arrange
        var dataFim = DateTime.Today.AddMonths(1);
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = dataFim,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetVigenteAsync(dataFim);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ComTabelas_DeveRetornarTodasOrdenadas()
    {
        // Arrange
        var tabelas = new List<TabelaPreco>
        {
            new() { DataInicioVigencia = DateTime.Today.AddMonths(-2), DataFimVigencia = DateTime.Today.AddMonths(-1), ValorHoraInicial = 8m, ValorHoraAdicional = 4m },
            new() { DataInicioVigencia = DateTime.Today.AddMonths(1), DataFimVigencia = DateTime.Today.AddMonths(2), ValorHoraInicial = 12m, ValorHoraAdicional = 6m },
            new() { DataInicioVigencia = DateTime.Today, DataFimVigencia = DateTime.Today.AddMonths(1), ValorHoraInicial = 10m, ValorHoraAdicional = 5m }
        };
        await _context.TabelasPreco.AddRangeAsync(tabelas);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var lista = result.ToList();
        lista.Should().HaveCount(3);
        lista[0].ValorHoraInicial.Should().Be(12m); // Mais recente primeiro
    }

    [Fact]
    public async Task GetAllAsync_SemTabelas_DeveRetornarListaVazia()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_TabelaExistente_DeveAtualizarComSucesso()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        tabela.ValorHoraInicial = 15m;
        tabela.ValorHoraAdicional = 7m;
        await _repository.UpdateAsync(tabela);
        await _context.SaveChangesAsync();

        // Assert
        var tabelaAtualizada = await _context.TabelasPreco.FindAsync(tabela.Id);
        tabelaAtualizada.Should().NotBeNull();
        tabelaAtualizada!.ValorHoraInicial.Should().Be(15m);
        tabelaAtualizada.ValorHoraAdicional.Should().Be(7m);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_TabelaExistente_DeveRemoverComSucesso()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();
        var id = tabela.Id;

        // Act
        await _repository.DeleteAsync(id);
        await _context.SaveChangesAsync();

        // Assert
        var tabelaRemovida = await _context.TabelasPreco.FindAsync(id);
        tabelaRemovida.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_TabelaNaoExistente_NaoDeveLancarExcecao()
    {
        // Act
        var action = async () => await _repository.DeleteAsync(999);

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsConflitanteAsync

    [Fact]
    public async Task ExistsConflitanteAsync_SemConflito_DeveRetornarFalse()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today.AddMonths(2),
            DateTime.Today.AddMonths(3));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsConflitanteAsync_ComConflitoInicio_DeveRetornarTrue()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act - Nova tabela começa dentro do período existente
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today.AddMonths(1),
            DateTime.Today.AddMonths(3));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsConflitanteAsync_ComConflitoFim_DeveRetornarTrue()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today.AddMonths(1),
            DataFimVigencia = DateTime.Today.AddMonths(3),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act - Nova tabela termina dentro do período existente
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today,
            DateTime.Today.AddMonths(2));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsConflitanteAsync_NovaTabelaEnvolveExistente_DeveRetornarTrue()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today.AddMonths(1),
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act - Nova tabela engloba totalmente a existente
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today,
            DateTime.Today.AddMonths(3));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsConflitanteAsync_ComExcludeId_DeveIgnorarTabela()
    {
        // Arrange
        var tabela = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        await _context.TabelasPreco.AddAsync(tabela);
        await _context.SaveChangesAsync();

        // Act - Verificar conflito excluindo a própria tabela
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today,
            DateTime.Today.AddMonths(2),
            tabela.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsConflitanteAsync_ComExcludeIdMasOutroConflita_DeveRetornarTrue()
    {
        // Arrange
        var tabela1 = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(2),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };
        var tabela2 = new TabelaPreco
        {
            DataInicioVigencia = DateTime.Today.AddMonths(3),
            DataFimVigencia = DateTime.Today.AddMonths(5),
            ValorHoraInicial = 12m,
            ValorHoraAdicional = 6m
        };
        await _context.TabelasPreco.AddRangeAsync(tabela1, tabela2);
        await _context.SaveChangesAsync();

        // Act - Atualizar tabela1 para período que conflita com tabela2
        var result = await _repository.ExistsConflitanteAsync(
            DateTime.Today.AddMonths(4),
            DateTime.Today.AddMonths(6),
            tabela1.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
