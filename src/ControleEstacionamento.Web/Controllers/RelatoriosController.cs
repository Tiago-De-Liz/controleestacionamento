using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControleEstacionamento.Web.Controllers;

public class RelatoriosController : Controller
{
    private readonly IEstacionamentoService _estacionamentoService;

    public RelatoriosController(IEstacionamentoService estacionamentoService)
    {
        _estacionamentoService = estacionamentoService;
    }

    public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
    {
        var fim = dataFim ?? DateTime.Today;
        var inicio = dataInicio ?? fim.AddDays(-30);

        var todosVeiculos = await _estacionamentoService.ListarTodosAsync();

        var veiculosPeriodo = todosVeiculos
            .Where(v => v.DataHoraEntrada.Date >= inicio.Date && v.DataHoraEntrada.Date <= fim.Date)
            .ToList();

        var finalizados = veiculosPeriodo.Where(v => v.DataHoraSaida.HasValue).ToList();

        var totalEntradas = veiculosPeriodo.Count;
        var totalSaidas = finalizados.Count;
        var receitaTotal = finalizados.Sum(v => v.ValorCobrado ?? 0);
        var ticketMedio = totalSaidas > 0 ? receitaTotal / totalSaidas : 0;
        var tempoMedio = totalSaidas > 0
            ? TimeSpan.FromMinutes(finalizados.Average(v => (v.DataHoraSaida!.Value - v.DataHoraEntrada).TotalMinutes))
            : TimeSpan.Zero;

        var ocupacaoPorDia = veiculosPeriodo
            .GroupBy(v => v.DataHoraEntrada.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Data = g.Key, Quantidade = g.Count() })
            .ToList();

        var receitaPorDia = finalizados
            .GroupBy(v => v.DataHoraSaida!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Data = g.Key, Valor = g.Sum(v => v.ValorCobrado ?? 0) })
            .ToList();

        var topPlacas = veiculosPeriodo
            .GroupBy(v => v.Placa)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { Placa = g.Key, Visitas = g.Count(), Total = g.Sum(v => v.ValorCobrado ?? 0) })
            .ToList();

        ViewBag.DataInicio = inicio.ToString("yyyy-MM-dd");
        ViewBag.DataFim = fim.ToString("yyyy-MM-dd");
        ViewBag.TotalEntradas = totalEntradas;
        ViewBag.TotalSaidas = totalSaidas;
        ViewBag.ReceitaTotal = receitaTotal;
        ViewBag.TicketMedio = ticketMedio;
        ViewBag.TempoMedio = tempoMedio;
        ViewBag.TopPlacas = topPlacas;

        ViewBag.OcupacaoLabels = ocupacaoPorDia.Select(o => o.Data.ToString("dd/MM")).ToList();
        ViewBag.OcupacaoValores = ocupacaoPorDia.Select(o => o.Quantidade).ToList();
        ViewBag.ReceitaLabels = receitaPorDia.Select(r => r.Data.ToString("dd/MM")).ToList();
        ViewBag.ReceitaValores = receitaPorDia.Select(r => r.Valor).ToList();

        return View(finalizados);
    }

    public async Task<IActionResult> ExportarPDF(DateTime? dataInicio, DateTime? dataFim)
    {
        var fim = dataFim ?? DateTime.Today;
        var inicio = dataInicio ?? fim.AddDays(-30);

        var todosVeiculos = await _estacionamentoService.ListarTodosAsync();
        var veiculosPeriodo = todosVeiculos
            .Where(v => v.DataHoraEntrada.Date >= inicio.Date && v.DataHoraEntrada.Date <= fim.Date)
            .ToList();

        var finalizados = veiculosPeriodo.Where(v => v.DataHoraSaida.HasValue).OrderBy(v => v.DataHoraEntrada).ToList();

        var totalEntradas = veiculosPeriodo.Count;
        var totalSaidas = finalizados.Count;
        var receitaTotal = finalizados.Sum(v => v.ValorCobrado ?? 0);
        var ticketMedio = totalSaidas > 0 ? receitaTotal / totalSaidas : 0;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, inicio, fim));
                page.Content().Element(c => ComposeContent(c, finalizados, totalEntradas, totalSaidas, receitaTotal, ticketMedio));
                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        var fileName = $"relatorio_financeiro_{inicio:yyyyMMdd}_{fim:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    private void ComposeHeader(IContainer container, DateTime inicio, DateTime fim)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("RELATÓRIO FINANCEIRO")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Sistema de Controle de Estacionamento")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(120).Column(col =>
                {
                    col.Item().AlignRight().Text($"Período:")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text($"{inicio:dd/MM/yyyy}")
                        .FontSize(11).Bold();
                    col.Item().AlignRight().Text($"a {fim:dd/MM/yyyy}")
                        .FontSize(11).Bold();
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, List<Application.DTOs.VeiculoResponseDto> finalizados,
        int totalEntradas, int totalSaidas, decimal receitaTotal, decimal ticketMedio)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                {
                    col.Item().Text("RESUMO").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Total de Entradas:");
                        r.ConstantItem(80).AlignRight().Text($"{totalEntradas}").Bold();
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total de Saídas:");
                        r.ConstantItem(80).AlignRight().Text($"{totalSaidas}").Bold();
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Receita Total:");
                        r.ConstantItem(80).AlignRight().Text($"R$ {receitaTotal:F2}").Bold().FontColor(Colors.Green.Darken2);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Ticket Médio:");
                        r.ConstantItem(80).AlignRight().Text($"R$ {ticketMedio:F2}").Bold();
                    });
                });
            });

            column.Item().Text("DETALHAMENTO").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5);

            if (finalizados.Any())
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(70);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Placa").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Entrada").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Saída").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Tempo").FontColor(Colors.White).Bold().FontSize(9);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight()
                            .Text("Valor").FontColor(Colors.White).Bold().FontSize(9);
                    });

                    var isAlternate = false;
                    foreach (var v in finalizados)
                    {
                        var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).Padding(4).Text(v.Placa).FontSize(9);
                        table.Cell().Background(bgColor).Padding(4).Text(v.DataHoraEntrada.ToString("dd/MM/yy HH:mm")).FontSize(9);
                        table.Cell().Background(bgColor).Padding(4).Text(v.DataHoraSaida?.ToString("dd/MM/yy HH:mm") ?? "-").FontSize(9);
                        table.Cell().Background(bgColor).Padding(4).Text(v.TempoEstadia).FontSize(9);
                        table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"R$ {v.ValorCobrado:F2}").FontSize(9);

                        isAlternate = !isAlternate;
                    }
                });
            }
            else
            {
                column.Item().PaddingVertical(20).AlignCenter()
                    .Text("Nenhum registro encontrado no período selecionado.")
                    .FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignLeft().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .FontSize(8).FontColor(Colors.Grey.Darken1);

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }
}
