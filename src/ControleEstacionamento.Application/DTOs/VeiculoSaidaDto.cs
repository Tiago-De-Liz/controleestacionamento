namespace ControleEstacionamento.Application.DTOs;

public class VeiculoSaidaDto
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public DateTime DataHoraEntrada { get; set; }
    public DateTime DataHoraSaida { get; set; }
    public TimeSpan TempoEstadia { get; set; }
    public decimal ValorCobrado { get; set; }
}
