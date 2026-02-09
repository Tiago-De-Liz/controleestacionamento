namespace ControleEstacionamento.Application.Services.Strategies;

public interface ICalculoPrecoStrategy
{
    decimal CalcularValor(DateTime entrada, DateTime saida, decimal valorHoraInicial, decimal valorHoraAdicional);
}
