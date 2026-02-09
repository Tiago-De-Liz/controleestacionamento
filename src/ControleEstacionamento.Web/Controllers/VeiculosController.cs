using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Interfaces;
using System.Text;

namespace ControleEstacionamento.Web.Controllers;

public class VeiculosController : Controller
{
    private readonly IEstacionamentoService _estacionamentoService;

    public VeiculosController(IEstacionamentoService estacionamentoService)
    {
        _estacionamentoService = estacionamentoService;
    }

    public async Task<IActionResult> Index()
    {
        var veiculos = await _estacionamentoService.ListarVeiculosEstacionadosAsync();
        return View(veiculos);
    }

    public async Task<IActionResult> ListarEstacionadosPartial()
    {
        var veiculos = await _estacionamentoService.ListarVeiculosEstacionadosAsync();
        return PartialView("_VeiculosTable", veiculos);
    }

    public async Task<IActionResult> Historico(string? placa, DateTime? dataInicio, DateTime? dataFim, int pagina = 1, int tamanhoPagina = 10)
    {
        var veiculos = await _estacionamentoService.ListarTodosAsync();

        if (!string.IsNullOrWhiteSpace(placa))
        {
            var placaNormalizada = placa.ToUpperInvariant().Replace("-", "");
            veiculos = veiculos.Where(v => v.Placa.Contains(placaNormalizada, StringComparison.OrdinalIgnoreCase));
        }

        if (dataInicio.HasValue)
        {
            veiculos = veiculos.Where(v => v.DataHoraEntrada.Date >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            veiculos = veiculos.Where(v => v.DataHoraEntrada.Date <= dataFim.Value.Date);
        }

        var veiculosOrdenados = veiculos.OrderByDescending(v => v.DataHoraEntrada);
        var paginatedList = PaginatedList<VeiculoResponseDto>.Create(veiculosOrdenados, pagina, tamanhoPagina);

        ViewBag.Placa = placa;
        ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
        ViewBag.TamanhoPagina = tamanhoPagina;

        return View(paginatedList);
    }

    public async Task<IActionResult> ExportarCSV(string? placa, DateTime? dataInicio, DateTime? dataFim)
    {
        var veiculos = await _estacionamentoService.ListarTodosAsync();

        if (!string.IsNullOrWhiteSpace(placa))
        {
            var placaNormalizada = placa.ToUpperInvariant().Replace("-", "");
            veiculos = veiculos.Where(v => v.Placa.Contains(placaNormalizada, StringComparison.OrdinalIgnoreCase));
        }

        if (dataInicio.HasValue)
        {
            veiculos = veiculos.Where(v => v.DataHoraEntrada.Date >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            veiculos = veiculos.Where(v => v.DataHoraEntrada.Date <= dataFim.Value.Date);
        }

        var sb = new StringBuilder();
        sb.AppendLine("Placa;Data Entrada;Data Saída;Tempo Estadia;Valor Cobrado;Status");

        foreach (var v in veiculos)
        {
            var status = v.DataHoraSaida.HasValue ? "Finalizado" : "Estacionado";
            var saida = v.DataHoraSaida?.ToString("dd/MM/yyyy HH:mm") ?? "";
            var valor = v.ValorCobrado?.ToString("F2") ?? "";

            sb.AppendLine($"{v.Placa};{v.DataHoraEntrada:dd/MM/yyyy HH:mm};{saida};{v.TempoEstadia};{valor};{status}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"historico_estacionamento_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    public IActionResult Entrada()
    {
        return View(new VeiculoEntradaDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrada(VeiculoEntradaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _estacionamentoService.RegistrarEntradaAsync(dto);
            TempData["Sucesso"] = "Entrada registrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarSaida(int id)
    {
        try
        {
            var resultado = await _estacionamentoService.RegistrarSaidaAsync(id);
            TempData["Sucesso"] = $"Saída registrada! Valor cobrado: R$ {resultado.ValorCobrado:F2}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarSaidaAjax(int id)
    {
        try
        {
            var resultado = await _estacionamentoService.RegistrarSaidaAsync(id);

            return Json(new
            {
                success = true,
                placa = resultado.Placa,
                entrada = resultado.DataHoraEntrada.ToString("dd/MM/yyyy HH:mm"),
                saida = resultado.DataHoraSaida.ToString("dd/MM/yyyy HH:mm"),
                tempo = resultado.TempoEstadia,
                valor = $"R$ {resultado.ValorCobrado:F2}"
            });
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        var veiculo = await _estacionamentoService.BuscarPorIdAsync(id);

        if (veiculo == null)
        {
            return NotFound();
        }

        return View(veiculo);
    }
}
