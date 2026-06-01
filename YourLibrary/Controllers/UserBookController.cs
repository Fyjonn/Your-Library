using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize] // Dostęp tylko dla zalogowanych użytkowników
    public class UserBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserBookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserBook { Book = new Book() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserBook userbook)
        {
            // 1. Pobieramy ID zalogowanego użytkownika i przypisujemy do rekordu
            string currentUserId = _userManager.GetUserId(User);
            userbook.ApplicationUserId = currentUserId;

            // 2. Wyciągamy wartości bezpośrednio z formularza (odporność na pola readonly API)
            var formTitle = Request.Form["Book.Title"].ToString();
            var formAuthor = Request.Form["Book.Author"].ToString();

            if (userbook.Book == null)
            {
                userbook.Book = new Book();
            }
            userbook.Book.Title = formTitle;
            userbook.Book.Author = formAuthor;
            userbook.Book.ReviewId = null; // Zabezpieczenie przed brakiem powiązanej recenzji

            // 3. Sprawdzamy kluczowy warunek – jeśli mamy tytuł, możemy bezpiecznie zapisywać
            if (!string.IsNullOrEmpty(userbook.Book.Title))
            {
                // Najpierw zapisujemy samą książkę do tabeli Books
                _context.Books.Add(userbook.Book);
                _context.SaveChanges();

                // Przypisujemy wygenerowane BookId do naszej relacji użytkownika
                userbook.BookId = userbook.Book.BookId;

                // Teraz zapisujemy rekord na półce użytkownika
                _context.UserBooks.Add(userbook);
                _context.SaveChanges();

                // Przekierowanie na półkę
                return RedirectToAction("Index", "Shelf");
            }

            // Jeśli tytuł był pusty, dorzucamy błąd do widoku i wracamy do formularza
            ModelState.AddModelError("Book.Title", "Tytuł książki nie może być pusty.");
            return View(userbook);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .FirstOrDefault(ub => ub.UserBookId == id && ub.ApplicationUserId == currentUserId);

            // Zabezpieczenie przed brakiem rekordu lub usuniętą książką
            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            return View(userBook);
        }
    }
}