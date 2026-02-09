using ControleEstacionamento.Application.Services.Strategies;
using FluentAssertions;

namespace ControleEstacionamento.Tests.Unit.Services;

public class CalculoPrecoStrategyTests
{
    private readonly ICalculoPrecoStrategy _strategy;
    private readonly decimal _valorHoraInicial = 10.00m;
    private readonly decimal _valorHoraAdicional = 5.00m;

    public CalculoPrecoStrategyTests()
    {
        _strategy = new CalculoPrecoStrategy();
    }

    [Fact]
    public void CalcularValor_Ate15Minutos_DeveRetornarMetadeHoraInicial()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(15);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(5.00m); // metade da hora inicial
    }

    [Fact]
    public void CalcularValor_30Minutos_DeveRetornarMetadeHoraInicial()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(30);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(5.00m); // metade da hora inicial
    }

    [Fact]
    public void CalcularValor_31Minutos_DeveRetornarHoraInicial()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(31);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(10.00m); // hora inicial completa
    }

    [Fact]
    public void CalcularValor_60Minutos_DeveRetornarHoraInicial()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(60);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(10.00m); // hora inicial completa
    }

    [Fact]
    public void CalcularValor_1Hora10Minutos_DeveRetornarHoraInicial_DentroTolerancia()
    {
        // Arrange - 1h10 está dentro da tolerância de 10 min
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(70); // 1h10

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(10.00m); // ainda 1 hora (tolerância de 10 min)
    }

    [Fact]
    public void CalcularValor_1Hora15Minutos_DeveRetornar2Horas()
    {
        // Arrange - 1h15 passa da tolerância de 10 min
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(75); // 1h15

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(15.00m); // hora inicial + 1 hora adicional
    }

    [Fact]
    public void CalcularValor_2Horas5Minutos_DeveRetornar2Horas_DentroTolerancia()
    {
        // Arrange - 2h05 está dentro da tolerância de 20 min (10 min por hora adicional)
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(125); // 2h05

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(15.00m); // hora inicial + 1 hora adicional
    }

    [Fact]
    public void CalcularValor_2Horas15Minutos_DeveRetornar3Horas()
    {
        // Arrange - 2h15 passa da tolerância de 20 min
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(135); // 2h15

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(20.00m); // hora inicial + 2 horas adicionais
    }

    [Fact]
    public void CalcularValor_3Horas_DeveRetornar3Horas()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddHours(3);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(20.00m); // hora inicial + 2 horas adicionais
    }

    [Fact]
    public void CalcularValor_5Horas_DeveRetornar5Horas()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddHours(5);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(30.00m); // hora inicial (10) + 4 horas adicionais (4*5=20)
    }

    [Fact]
    public void CalcularValor_ValoresPersonalizados_DeveCalcularCorretamente()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddHours(3);
        var horaInicial = 20.00m;
        var horaAdicional = 8.00m;

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, horaInicial, horaAdicional);

        // Assert
        resultado.Should().Be(36.00m); // 20 + (2 * 8)
    }

    [Fact]
    public void CalcularValor_MesmaMomentoDeSaida_DeveRetornarZero()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada;

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(0m);
    }

    [Fact]
    public void CalcularValor_1Minuto_DeveRetornarMetadeHoraInicial()
    {
        // Arrange
        var entrada = new DateTime(2024, 1, 1, 10, 0, 0);
        var saida = entrada.AddMinutes(1);

        // Act
        var resultado = _strategy.CalcularValor(entrada, saida, _valorHoraInicial, _valorHoraAdicional);

        // Assert
        resultado.Should().Be(5.00m); // metade da hora inicial
    }
}
