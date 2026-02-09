namespace ControleEstacionamento.Domain.Entities;

public class TabelaPreco
{
    public int Id { get; set; }
    public DateTime DataInicioVigencia { get; set; }
    public DateTime DataFimVigencia { get; set; }
    public decimal ValorHoraInicial { get; set; }
    public decimal ValorHoraAdicional { get; set; }
}
