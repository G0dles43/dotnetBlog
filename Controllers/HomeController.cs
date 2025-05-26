using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Models;

namespace BlogApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> DailyJoke()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://v2.jokeapi.dev/joke/Any?format=json");

        if (!response.IsSuccessStatusCode)
            return Json(new { error = "Error" });

        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

}
