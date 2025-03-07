using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BW1_E_commerce.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace BW1_E_commerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

  public IActionResult AddDelete()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [Route("Error/{statusCode}")]
    public IActionResult Error(int statusCode)
    {
        if (statusCode == 404)
        {
            ViewBag.ErrorMessage = "La pagina che stai cercando non esiste.";
        }
        else if (statusCode == 500)
        {
            ViewBag.ErrorMessage = "Si è verificato un errore interno del server.";
        }
        else
        {
            ViewBag.ErrorMessage = "Si è verificato un errore.";
        }

        return View();
    }


}
