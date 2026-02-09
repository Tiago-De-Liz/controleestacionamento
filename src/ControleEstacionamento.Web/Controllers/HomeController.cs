using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Web.Models;
using ControleEstacionamento.Application.Interfaces;

namespace ControleEstacionamento.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEstacionamentoService _estacionamentoService;
    private readonly ITabelaPrecoService _tabelaPrecoService;

    public HomeController(IEstacionamentoService estacionamentoService, ITabelaPrecoService tabelaPrecoService)
    {
        _estacionamentoService = estacionamentoService;
        _tabelaPrecoService = tabelaPrecoService;
    }

    public async Task<IActionResult> Index()
    {
        var veiculosEstacionados = await _estacionamentoService.ListarVeiculosEstacionadosAsync();
        var tabelaVigente = await _tabelaPrecoService.BuscarVigenteAsync();

        ViewBag.TabelaVigente = tabelaVigente;
        ViewBag.TotalVeiculos = veiculosEstacionados.Count();

        return View(veiculosEstacionados);
    }

    public async Task<IActionResult> ListarVeiculosPartial()
    {
        var veiculosEstacionados = await _estacionamentoService.ListarVeiculosEstacionadosAsync();
        return PartialView("_PainelVeiculos", veiculosEstacionados);
    }

    public async Task<IActionResult> ContarVeiculos()
    {
        var veiculosEstacionados = await _estacionamentoService.ListarVeiculosEstacionadosAsync();
        return Json(new { total = veiculosEstacionados.Count() });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
