using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string BackendUrl = "http://localhost:5000"; // Will be updated for production

    public HomeController()
    {
        _httpClient = new HttpClient();
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new {
            message = "BalanceHub Web API is running!",
            timestamp = DateTime.UtcNow,
            backend_url = BackendUrl
        });
    }
}
