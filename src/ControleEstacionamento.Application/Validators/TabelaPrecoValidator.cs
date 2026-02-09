using ControleEstacionamento.Application.DTOs;
using FluentValidation;

namespace ControleEstacionamento.Application.Validators;

public class TabelaPrecoCreateValidator : AbstractValidator<TabelaPrecoCreateDto>
{
    public TabelaPrecoCreateValidator()
    {
        RuleFor(x => x.DataInicioVigencia)
            .NotEmpty().WithMessage("A data de início da vigência é obrigatória.");

        RuleFor(x => x.DataFimVigencia)
            .NotEmpty().WithMessage("A data de fim da vigência é obrigatória.")
            .GreaterThan(x => x.DataInicioVigencia)
                .WithMessage("A data de fim deve ser maior que a data de início.");

        RuleFor(x => x.ValorHoraInicial)
            .GreaterThan(0).WithMessage("O valor da hora inicial deve ser maior que zero.");

        RuleFor(x => x.ValorHoraAdicional)
            .GreaterThan(0).WithMessage("O valor da hora adicional deve ser maior que zero.");
    }
}

public class TabelaPrecoUpdateValidator : AbstractValidator<TabelaPrecoUpdateDto>
{
    public TabelaPrecoUpdateValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O ID é obrigatório.");

        RuleFor(x => x.DataInicioVigencia)
            .NotEmpty().WithMessage("A data de início da vigência é obrigatória.");

        RuleFor(x => x.DataFimVigencia)
            .NotEmpty().WithMessage("A data de fim da vigência é obrigatória.")
            .GreaterThan(x => x.DataInicioVigencia)
                .WithMessage("A data de fim deve ser maior que a data de início.");

        RuleFor(x => x.ValorHoraInicial)
            .GreaterThan(0).WithMessage("O valor da hora inicial deve ser maior que zero.");

        RuleFor(x => x.ValorHoraAdicional)
            .GreaterThan(0).WithMessage("O valor da hora adicional deve ser maior que zero.");
    }
}
