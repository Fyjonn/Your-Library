using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YourLibrary.Data;
using YourLibrary.Models;


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

                return RedirectToAction("Index", "Shelf");
            }
            return View(userbook);
        }

        // za skomplikowane żeby zrobić go automatycznie tzreba będzie klepać ręcznie

        [HttpGet]
        public IActionResult Index()
        {
            string currentUserId = _userManager.GetUserId(User);
            var myBooks = _context.UserBooks
                .Include(ub => ub.Book)
                .Where(ub => ub.ApplicationUserId == currentUserId)
                .ToList();
            return View(myBooks);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .FirstOrDefault(ub => ub.UserBookId == id && ub.ApplicationUserId == currentUserId);

            if (userBook == null)
            {
                return NotFound();
            }

            return View(userBook);

        }
    }
}
