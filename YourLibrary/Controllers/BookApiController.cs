using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YourLibrary.Data;

/// <summary>
/// Kontroler odpowiadajacy za interakcje z Google Books API. Pozwala na wyszukiwanie ksiazek na podstawie tytulu
/// lub autora.
/// </summary>

namespace YourLibrary.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public BookApiController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }


        // wyszukiwanie
        [HttpGet("search")]
        public IActionResult SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty");
            // bez polskich znakow
            string cleanQuery = RemoveDiacritics(query.Trim());

            // tytul lub autor
            string smartQuery = $"{cleanQuery} OR inauthor:{cleanQuery} OR intitle:{cleanQuery}";

            string apiKey = _configuration["GoogleBooksApi:ApiKey"];

            string url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(smartQuery)}&key={apiKey}&maxResults=8";

            var client = _httpClientFactory.CreateClient();
            var response = client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error fetching data from Google Books");

            var content = response.Content.ReadAsStringAsync().Result;

            return Content(content, "application/json");
        }

        // funkcja usuwajaca polskie znaki
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}
