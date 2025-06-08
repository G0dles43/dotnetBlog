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
        const string sessionKey = "DailyJoke";
        const string dateKey = "DailyJokeDate";
        var today = DateTime.UtcNow.Date;

        if (HttpContext.Session.GetString(sessionKey) is string storedJoke &&
            HttpContext.Session.GetString(dateKey) == today.ToString("yyyy-MM-dd"))
        {
            return Content(storedJoke, "application/json");
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://v2.jokeapi.dev/joke/Any?format=json");

        if (!response.IsSuccessStatusCode)
            return Json(new { error = "Error" });

        var json = await response.Content.ReadAsStringAsync();

        HttpContext.Session.SetString(sessionKey, json);
        HttpContext.Session.SetString(dateKey, today.ToString("yyyy-MM-dd"));

        return Content(json, "application/json");
    }
}
