using ControleEstacionamento.Application.DTOs;
using FluentValidation;

namespace ControleEstacionamento.Application.Validators;

public class VeiculoEntradaValidator : AbstractValidator<VeiculoEntradaDto>
{
    public VeiculoEntradaValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("A placa é obrigatória.")
            .MinimumLength(7).WithMessage("A placa deve ter no mínimo 7 caracteres.")
            .MaximumLength(10).WithMessage("A placa deve ter no máximo 10 caracteres.")
            .Matches(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$")
                .WithMessage("A placa deve estar no formato válido (ABC1234 ou ABC1D23).");
    }
}
