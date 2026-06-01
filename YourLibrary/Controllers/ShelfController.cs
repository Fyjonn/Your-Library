using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize] // Tylko zalogowani użytkownicy mogą zobaczyć swoją półkę
    public class ShelfController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShelfController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // 1. Pobieramy ID zalogowanego użytkownika
            string currentUserId = _userManager.GetUserId(User);

            // 2. Wyciągamy z bazy jego książki razem z danymi o tytule/autorze
            var myBooks = _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.ApplicationUserId == currentUserId)
                .ToList();

            // 3. Przekazujemy listę do widoku Shelf/Index.cshtml (Kluczowa poprawka!)
            return View(myBooks);
        }
    }
}