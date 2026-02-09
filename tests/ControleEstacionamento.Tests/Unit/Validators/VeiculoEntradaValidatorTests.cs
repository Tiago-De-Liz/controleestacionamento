using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleEstacionamento.Tests.Unit.Validators;

public class VeiculoEntradaValidatorTests
{
    private readonly VeiculoEntradaValidator _validator;

    public VeiculoEntradaValidatorTests()
    {
        _validator = new VeiculoEntradaValidator();
    }

    #region Placa Válida

    [Theory]
    [InlineData("ABC1234")] // Formato antigo
    [InlineData("ABC1D23")] // Formato Mercosul
    [InlineData("XYZ9A87")]
    [InlineData("QWE4R56")]
    public void Validate_PlacaValida_DevePassar(string placa)
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = placa };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Placa Vazia ou Nula

    [Fact]
    public void Validate_PlacaVazia_DeveFalhar()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Placa)
            .WithErrorMessage("A placa é obrigatória.");
    }

    [Fact]
    public void Validate_PlacaComEspacos_DeveFalhar()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "   " };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Placa);
    }

    #endregion

    #region Comprimento da Placa

    [Theory]
    [InlineData("ABC123")] // 6 caracteres - muito curto
    [InlineData("AB1234")] // 6 caracteres
    public void Validate_PlacaMuitoCurta_DeveFalhar(string placa)
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = placa };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Placa)
            .WithErrorMessage("A placa deve ter no mínimo 7 caracteres.");
    }

    [Theory]
    [InlineData("ABC1234567890")] // 13 caracteres - muito longo
    public void Validate_PlacaMuitoLonga_DeveFalhar(string placa)
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = placa };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Placa)
            .WithErrorMessage("A placa deve ter no máximo 10 caracteres.");
    }

    #endregion

    #region Formato da Placa

    [Theory]
    [InlineData("1234567")] // Só números
    [InlineData("ABCDEFG")] // Só letras
    [InlineData("123ABCD")] // Ordem invertida
    [InlineData("AB12345")] // Formato incorreto
    [InlineData("ABCD123")] // 4 letras no início
    [InlineData("ABC12D3")] // Formato incorreto
    [InlineData("abc1d23")] // Minúsculas
    public void Validate_FormatoInvalido_DeveFalhar(string placa)
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = placa };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Placa)
            .WithErrorMessage("A placa deve estar no formato válido (ABC1234 ou ABC1D23).");
    }

    #endregion

    #region Múltiplos Erros

    [Fact]
    public void Validate_PlacaVaziaEInvalida_DeveRetornarMultiplosErros()
    {
        // Arrange
        var dto = new VeiculoEntradaDto { Placa = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.Placa);
    }

    #endregion
}
