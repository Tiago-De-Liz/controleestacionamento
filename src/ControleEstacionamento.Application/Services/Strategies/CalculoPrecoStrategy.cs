namespace ControleEstacionamento.Application.Services.Strategies;

public class CalculoPrecoStrategy : ICalculoPrecoStrategy
{
    private const int TOLERANCIA_MINUTOS_POR_HORA = 10;
    private const int MINUTOS_MEIA_HORA = 30;
    private const int MINUTOS_POR_HORA = 60;

    public decimal CalcularValor(DateTime entrada, DateTime saida, decimal valorHoraInicial, decimal valorHoraAdicional)
    {
        var tempoEstadia = saida - entrada;
        var minutosEstadia = (int)tempoEstadia.TotalMinutes;

        if (minutosEstadia <= 0)
        {
            return 0m;
        }

        if (minutosEstadia <= MINUTOS_MEIA_HORA)
        {
            return valorHoraInicial / 2;
        }

        if (minutosEstadia <= MINUTOS_POR_HORA)
        {
            return valorHoraInicial;
        }

        return CalcularValorComHorasAdicionais(minutosEstadia, valorHoraInicial, valorHoraAdicional);
    }

    private decimal CalcularValorComHorasAdicionais(int minutosEstadia, decimal valorHoraInicial, decimal valorHoraAdicional)
    {
        var minutosAlemPrimeiraHora = minutosEstadia - MINUTOS_POR_HORA;
        var horasAdicionais = CalcularHorasAdicionaisComTolerancia(minutosAlemPrimeiraHora);

        return valorHoraInicial + (horasAdicionais * valorHoraAdicional);
    }

    private int CalcularHorasAdicionaisComTolerancia(int minutosAlemPrimeiraHora)
    {
        if (minutosAlemPrimeiraHora <= 0)
        {
            return 0;
        }

        var horasAdicionaisCheias = minutosAlemPrimeiraHora / MINUTOS_POR_HORA;
        var minutosRestantes = minutosAlemPrimeiraHora % MINUTOS_POR_HORA;

        if (minutosRestantes == 0)
        {
            return horasAdicionaisCheias;
        }

        var tolerancia = (horasAdicionaisCheias == 0 ? 1 : horasAdicionaisCheias) * TOLERANCIA_MINUTOS_POR_HORA;

        if (minutosRestantes > tolerancia)
        {
            return horasAdicionaisCheias + 1;
        }

        return horasAdicionaisCheias == 0 ? 0 : horasAdicionaisCheias;
    }
}
