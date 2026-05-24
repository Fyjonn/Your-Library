using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourLibrary.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookApiController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("search")]
        public IActionResult SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty");

            string apiKey = _configuration["GoogleBooksApi:ApiKey"];

            string url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&key={apiKey}&maxResults=5";

            var client = _httpClientFactory.CreateClient();
            var response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error fetching data from Google Books");

            var content = response.Content.ReadAsStringAsync().Result;

            // json
            return Content(content, "application/json");
        }
    }
}
