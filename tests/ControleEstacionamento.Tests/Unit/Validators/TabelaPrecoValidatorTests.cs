using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleEstacionamento.Tests.Unit.Validators;

public class TabelaPrecoCreateValidatorTests
{
    private readonly TabelaPrecoCreateValidator _validator;

    public TabelaPrecoCreateValidatorTests()
    {
        _validator = new TabelaPrecoCreateValidator();
    }

    #region Dados Válidos

    [Fact]
    public void Validate_DadosValidos_DevePassar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_DadosValidosComValoresAltos_DevePassar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddYears(1),
            ValorHoraInicial = 100m,
            ValorHoraAdicional = 50m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region DataInicioVigencia

    [Fact]
    public void Validate_DataInicioVigenciaVazia_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = default,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataInicioVigencia)
            .WithErrorMessage("A data de início da vigência é obrigatória.");
    }

    #endregion

    #region DataFimVigencia

    [Fact]
    public void Validate_DataFimVigenciaVazia_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = default,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataFimVigencia)
            .WithErrorMessage("A data de fim da vigência é obrigatória.");
    }

    [Fact]
    public void Validate_DataFimMenorQueDataInicio_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddDays(-1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataFimVigencia)
            .WithErrorMessage("A data de fim deve ser maior que a data de início.");
    }

    [Fact]
    public void Validate_DataFimIgualDataInicio_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataFimVigencia)
            .WithErrorMessage("A data de fim deve ser maior que a data de início.");
    }

    #endregion

    #region ValorHoraInicial

    [Fact]
    public void Validate_ValorHoraInicialZero_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 0m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraInicial)
            .WithErrorMessage("O valor da hora inicial deve ser maior que zero.");
    }

    [Fact]
    public void Validate_ValorHoraInicialNegativo_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = -10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraInicial)
            .WithErrorMessage("O valor da hora inicial deve ser maior que zero.");
    }

    #endregion

    #region ValorHoraAdicional

    [Fact]
    public void Validate_ValorHoraAdicionalZero_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 0m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraAdicional)
            .WithErrorMessage("O valor da hora adicional deve ser maior que zero.");
    }

    [Fact]
    public void Validate_ValorHoraAdicionalNegativo_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = -5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraAdicional)
            .WithErrorMessage("O valor da hora adicional deve ser maior que zero.");
    }

    #endregion

    #region Múltiplos Erros

    [Fact]
    public void Validate_TodosOsCamposInvalidos_DeveRetornarMultiplosErros()
    {
        // Arrange
        var dto = new TabelaPrecoCreateDto
        {
            DataInicioVigencia = default,
            DataFimVigencia = default,
            ValorHoraInicial = 0m,
            ValorHoraAdicional = 0m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion
}

public class TabelaPrecoUpdateValidatorTests
{
    private readonly TabelaPrecoUpdateValidator _validator;

    public TabelaPrecoUpdateValidatorTests()
    {
        _validator = new TabelaPrecoUpdateValidator();
    }

    #region Dados Válidos

    [Fact]
    public void Validate_DadosValidos_DevePassar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Id

    [Fact]
    public void Validate_IdZero_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 0,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O ID é obrigatório.");
    }

    [Fact]
    public void Validate_IdNegativo_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = -1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("O ID é obrigatório.");
    }

    #endregion

    #region DataInicioVigencia

    [Fact]
    public void Validate_DataInicioVigenciaVazia_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = default,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataInicioVigencia)
            .WithErrorMessage("A data de início da vigência é obrigatória.");
    }

    #endregion

    #region DataFimVigencia

    [Fact]
    public void Validate_DataFimVigenciaVazia_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = default,
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataFimVigencia)
            .WithErrorMessage("A data de fim da vigência é obrigatória.");
    }

    [Fact]
    public void Validate_DataFimMenorQueDataInicio_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddDays(-1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataFimVigencia)
            .WithErrorMessage("A data de fim deve ser maior que a data de início.");
    }

    #endregion

    #region ValorHoraInicial

    [Fact]
    public void Validate_ValorHoraInicialZero_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 0m,
            ValorHoraAdicional = 5m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraInicial)
            .WithErrorMessage("O valor da hora inicial deve ser maior que zero.");
    }

    #endregion

    #region ValorHoraAdicional

    [Fact]
    public void Validate_ValorHoraAdicionalZero_DeveFalhar()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 1,
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddMonths(1),
            ValorHoraInicial = 10m,
            ValorHoraAdicional = 0m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHoraAdicional)
            .WithErrorMessage("O valor da hora adicional deve ser maior que zero.");
    }

    #endregion

    #region Múltiplos Erros

    [Fact]
    public void Validate_TodosOsCamposInvalidos_DeveRetornarMultiplosErros()
    {
        // Arrange
        var dto = new TabelaPrecoUpdateDto
        {
            Id = 0,
            DataInicioVigencia = default,
            DataFimVigencia = default,
            ValorHoraInicial = 0m,
            ValorHoraAdicional = 0m
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    #endregion
}
