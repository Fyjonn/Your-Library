using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using YourLibrary.Data;
using YourLibrary.Models;
using System.Globalization;
using System.Text;

namespace YourLibrary.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(int userBookId, decimal rating, string reviewComment)
        {
            if (rating < 1 || rating > 10)
            {
                return BadRequest("Invalid rating value.");
            }

            var userBook = await _context.UserBooks.FindAsync(userBookId);
            if (userBook == null)
            {
                return NotFound("Associated UserBook not found.");
            }

            var review = new Review
            {
                Rating = rating,
                ReviewComment = reviewComment,
                UserBook = userBook
            };

            _context.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Shelf");
        }

        [HttpGet("api/reviews")]
        public IActionResult GetBookReviews([FromQuery] string title, [FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title cannot be empty");

            string cleanSearchTitle = RemoveDiacritics(title.Trim().ToLower());

            var allReviews = _context.Reviews
                            .Include(r => r.UserBook)
                                .ThenInclude(ub => ub.Book)
                            .Include(r => r.UserBook.ApplicationUser)
                            .Where(r => r.UserBook.Book != null && !string.IsNullOrEmpty(r.ReviewComment))
                            .ToList();

            var filteredReviews = allReviews
                .Where(r => RemoveDiacritics(r.UserBook.Book.Title.Trim().ToLower()) == cleanSearchTitle)
                .OrderByDescending(r => r.ReviewId)
                .Select(r => new
                {
                    user = r.UserBook.ApplicationUser != null ? r.UserBook.ApplicationUser.UserName : "Reader",
                    rating = r.Rating,
                    comment = r.ReviewComment,
                    date = DateTime.Now.ToString("yyyy-MM-dd")
                })
                .Take(10)
                .ToList();

            return Ok(filteredReviews);
        }

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
