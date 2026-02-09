namespace ControleEstacionamento.Domain.Entities;

public class VeiculoEstacionado
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public DateTime DataHoraEntrada { get; set; }
    public DateTime? DataHoraSaida { get; set; }
    public decimal? ValorCobrado { get; set; }
}
