using ControleEstacionamento.Domain.Entities;

namespace ControleEstacionamento.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.TabelasPreco.Any())
        {
            context.TabelasPreco.Add(new TabelaPreco
            {
                ValorHoraInicial = 10.00m,
                ValorHoraAdicional = 5.00m,
                DataInicioVigencia = DateTime.Today.AddMonths(-1),
                DataFimVigencia = DateTime.Today.AddYears(1)
            });
            context.SaveChanges();
        }

        if (context.VeiculosEstacionados.Count() < 10)
        {
            var random = new Random(42);
            var placas = GeneratePlacas(50);
            var veiculos = new List<VeiculoEstacionado>();

            for (int i = 0; i < 100; i++)
            {
                var placa = placas[random.Next(placas.Count)];
                var diasAtras = random.Next(0, 30);
                var horaEntrada = random.Next(6, 22);
                var minutoEntrada = random.Next(0, 60);

                var dataEntrada = DateTime.Today.AddDays(-diasAtras)
                    .AddHours(horaEntrada)
                    .AddMinutes(minutoEntrada);

                DateTime? dataSaida = null;
                decimal? valorCobrado = null;

                if (random.NextDouble() < 0.8 || diasAtras > 0)
                {
                    var minutosEstacionado = random.Next(15, 480);
                    dataSaida = dataEntrada.AddMinutes(minutosEstacionado);
                    valorCobrado = CalcularValor(minutosEstacionado);
                }

                veiculos.Add(new VeiculoEstacionado
                {
                    Placa = placa,
                    DataHoraEntrada = dataEntrada,
                    DataHoraSaida = dataSaida,
                    ValorCobrado = valorCobrado
                });
            }

            var placasEstacionadas = new[] { "ABC1D23", "XYZ9A87", "QWE4R56", "RTY7U89", "FGH2J34" };
            foreach (var placa in placasEstacionadas)
            {
                var horasAtras = random.Next(0, 5);
                veiculos.Add(new VeiculoEstacionado
                {
                    Placa = placa,
                    DataHoraEntrada = DateTime.Now.AddHours(-horasAtras).AddMinutes(-random.Next(0, 60)),
                    DataHoraSaida = null,
                    ValorCobrado = null
                });
            }

            context.VeiculosEstacionados.AddRange(veiculos);
            context.SaveChanges();
        }
    }

    private static List<string> GeneratePlacas(int count)
    {
        var placas = new List<string>();
        var random = new Random(123);
        var letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var numeros = "0123456789";

        for (int i = 0; i < count; i++)
        {
            var placa = $"{letras[random.Next(26)]}{letras[random.Next(26)]}{letras[random.Next(26)]}" +
                       $"{numeros[random.Next(10)]}{letras[random.Next(26)]}{numeros[random.Next(10)]}{numeros[random.Next(10)]}";
            placas.Add(placa);
        }

        return placas;
    }

    private static decimal CalcularValor(int minutos)
    {
        const decimal valorHoraInicial = 10.00m;
        const decimal valorHoraAdicional = 5.00m;
        const int toleranciaMinutos = 10;

        if (minutos <= 30)
            return valorHoraInicial / 2;

        if (minutos <= 60)
            return valorHoraInicial;

        var minutosAposHoraInicial = minutos - 60;
        var horasAdicionaisCheias = minutosAposHoraInicial / 60;
        var minutosRestantes = minutosAposHoraInicial % 60;

        var tolerancia = (horasAdicionaisCheias == 0 ? 1 : horasAdicionaisCheias) * toleranciaMinutos;

        if (minutosRestantes > tolerancia)
            horasAdicionaisCheias++;

        return valorHoraInicial + (horasAdicionaisCheias * valorHoraAdicional);
    }
}
