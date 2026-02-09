namespace ControleEstacionamento.Application.DTOs;

public class TabelaPrecoDto
{
    public int Id { get; set; }
    public DateTime DataInicioVigencia { get; set; }
    public DateTime DataFimVigencia { get; set; }
    public decimal ValorHoraInicial { get; set; }
    public decimal ValorHoraAdicional { get; set; }
}

public class TabelaPrecoCreateDto
{
    public DateTime DataInicioVigencia { get; set; }
    public DateTime DataFimVigencia { get; set; }
    public decimal ValorHoraInicial { get; set; }
    public decimal ValorHoraAdicional { get; set; }
}

public class TabelaPrecoUpdateDto
{
    public int Id { get; set; }
    public DateTime DataInicioVigencia { get; set; }
    public DateTime DataFimVigencia { get; set; }
    public decimal ValorHoraInicial { get; set; }
    public decimal ValorHoraAdicional { get; set; }
}
