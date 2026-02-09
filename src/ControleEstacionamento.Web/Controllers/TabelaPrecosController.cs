using Microsoft.AspNetCore.Mvc;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Application.Interfaces;

namespace ControleEstacionamento.Web.Controllers;

public class TabelaPrecosController : Controller
{
    private readonly ITabelaPrecoService _tabelaPrecoService;

    public TabelaPrecosController(ITabelaPrecoService tabelaPrecoService)
    {
        _tabelaPrecoService = tabelaPrecoService;
    }

    public async Task<IActionResult> Index()
    {
        var tabelas = await _tabelaPrecoService.ListarTodasAsync();
        return View(tabelas);
    }

    public IActionResult Criar()
    {
        return View(new TabelaPrecoCreateDto
        {
            DataInicioVigencia = DateTime.Today,
            DataFimVigencia = DateTime.Today.AddYears(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(TabelaPrecoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _tabelaPrecoService.CriarAsync(dto);
            TempData["Sucesso"] = "Tabela de preços criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        var tabela = await _tabelaPrecoService.BuscarPorIdAsync(id);

        if (tabela == null)
        {
            return NotFound();
        }

        var dto = new TabelaPrecoUpdateDto
        {
            Id = tabela.Id,
            DataInicioVigencia = tabela.DataInicioVigencia,
            DataFimVigencia = tabela.DataFimVigencia,
            ValorHoraInicial = tabela.ValorHoraInicial,
            ValorHoraAdicional = tabela.ValorHoraAdicional
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(TabelaPrecoUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var resultado = await _tabelaPrecoService.AtualizarAsync(dto);

            if (resultado == null)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Tabela de preços atualizada com sucesso!";
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
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _tabelaPrecoService.RemoverAsync(id);

        if (!resultado)
        {
            TempData["Erro"] = "Tabela de preços não encontrada.";
        }
        else
        {
            TempData["Sucesso"] = "Tabela de preços excluída com sucesso!";
        }

        return RedirectToAction(nameof(Index));
    }
}
