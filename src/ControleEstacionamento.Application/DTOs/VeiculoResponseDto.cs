namespace ControleEstacionamento.Application.DTOs;

public class VeiculoResponseDto
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public DateTime DataHoraEntrada { get; set; }
    public DateTime? DataHoraSaida { get; set; }
    public decimal? ValorCobrado { get; set; }
    public string TempoEstadia => DataHoraSaida.HasValue
        ? FormatarTempo(DataHoraSaida.Value - DataHoraEntrada)
        : FormatarTempo(DateTime.Now - DataHoraEntrada);

    private static string FormatarTempo(TimeSpan tempo)
    {
        if (tempo.TotalHours >= 1)
        {
            return $"{(int)tempo.TotalHours}h {tempo.Minutes}min";
        }
        return $"{tempo.Minutes}min";
    }
}
