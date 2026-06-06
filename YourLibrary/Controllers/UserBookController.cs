using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize] 
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
            string currentUserId = _userManager.GetUserId(User);
            userbook.ApplicationUserId = currentUserId;

            var formTitle = Request.Form["Book.Title"].ToString();
            var formAuthor = Request.Form["Book.Author"].ToString();

            if (userbook.Book == null)
            {
                userbook.Book = new Book();
            }
            userbook.Book.Title = formTitle;
            userbook.Book.Author = formAuthor;
            userbook.Book.ReviewId = null; 

            if (!string.IsNullOrEmpty(userbook.Book.Title))
            {
                _context.Books.Add(userbook.Book);
                _context.SaveChanges();

                userbook.BookId = userbook.Book.BookId;

                _context.UserBooks.Add(userbook);
                _context.SaveChanges();

                return RedirectToAction("Index", "Shelf");
            }

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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .FirstOrDefault(ub => ub.UserBookId == id && ub.ApplicationUserId == currentUserId);

            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            return View(userBook);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserBook userbook)
        {
            string currentUserId = _userManager.GetUserId(User);
            //userbook.ApplicationUserId = currentUserId;

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Book");
            ModelState.Remove("Review");
            ModelState.Remove("Borrows");

            if (!ModelState.IsValid)
            {
                userbook.Book = _context.Books.FirstOrDefault(b => b.BookId == userbook.BookId);
                return View(userbook);
            }

            var dbUserBook = _context.UserBooks.FirstOrDefault(ub => ub.UserBookId == userbook.UserBookId && ub.ApplicationUserId == currentUserId);

            if (dbUserBook == null)
            {
                return NotFound();
            }
            dbUserBook.ReadStatus = userbook.ReadStatus;
            dbUserBook.Location = userbook.Location;
            dbUserBook.Bookmark = userbook.Bookmark;
            dbUserBook.Notes = userbook.Notes;

            if (dbUserBook.IsBorrowed)
            {
                dbUserBook.IsOwned = false;
            }
            else
            {
                dbUserBook.Media = userbook.Media;
                dbUserBook.IsOwned = userbook.IsOwned;
            }

            //_context.UserBooks.Update(userbook);
            _context.SaveChanges();

            return RedirectToAction("Index", "Shelf"); //powrot do polki
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .FirstOrDefault(ub => ub.UserBookId == id && ub.ApplicationUserId == currentUserId);

            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            return View(userBook);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userbook = _context.UserBooks
                .FirstOrDefault(x => x.UserBookId == id && x.ApplicationUserId == currentUserId);

            if (userbook != null)
            {
                _context.UserBooks.Remove(userbook);
                _context.SaveChanges();
            }

            if (userbook.IsBorrowed)
            {
                TempData["FriendError"] = "You cannot delete a book that is currently borrowed!";
                return RedirectToAction("Index", "Shelf");
            }
           

            return RedirectToAction("Index", "Shelf");
        }
    }
}