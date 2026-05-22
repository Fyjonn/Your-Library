using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Models;
using YourLibrary.Data;


namespace YourLibrary.Controllers
{
    [Authorize] // zalogowani uzytkownicy
    public class UserBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        // wstrzykujemy
        public UserBookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Akcje

        // localhst:XXXX/UserBook/Create - żeby wejść na ten widok

        [HttpGet]
        public IActionResult Create()
        {
            // dodajemy tylko ksiazke na razie, nie cale userbook
            return View(new UserBook { Book = new Book() });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserBook userbook)
        {
            string currentUserId = _userManager.GetUserId(User);
            userbook.ApplicationUserId = currentUserId;

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Book");

            if (ModelState.IsValid)
            {
                if (userbook.Book != null && userbook.BookId == 0)
                {
                    _context.Books.Add(userbook.Book);
                    _context.SaveChanges();

                    userbook.BookId = userbook.Book.BookId;
                }

                _context.UserBooks.Add(userbook);
                _context.SaveChanges();

                return RedirectToAction(nameof(ViewAll));
            }
            return View(userbook);
        }

        // za skomplikowane żeby zrobić go automatycznie tzreba będzie klepać ręcznie

        [HttpGet]
        public IActionResult ViewAll()
        {
            string currentUserId = _userManager.GetUserId(User);
            var myBooks = _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.ApplicationUserId == currentUserId)
                .ToList();
            return View(myBooks);
        }
    }
}
