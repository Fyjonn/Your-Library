using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace YourLibrary.Controllers
{
    [Authorize] 
    public class UserBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserBookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserBook { Book = new Book() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserBook userbook, string reviewRating, string reviewComment, IFormFile coverFile)
        {
            string currentUserId = _userManager.GetUserId(User);
            userbook.ApplicationUserId = currentUserId;

            var formTitle = Request.Form["Book.Title"].ToString();
            var formAuthor = Request.Form["Book.Author"].ToString();
            var formGenre = Request.Form["Book.Genre"].ToString();
            var formAgeRating = Request.Form["Book.AgeRating"].ToString();
            var formDescription = Request.Form["Book.Description"].ToString();

            if (!string.IsNullOrEmpty(formTitle))
            {
                bool alreadyExists = _context.UserBooks
                    .Include(ub => ub.Book)
                    .Any(ub => ub.ApplicationUserId == currentUserId
                            && ub.Book.Title.ToLower() == formTitle.ToLower()
                            && ub.Book.Author.ToLower() == formAuthor.ToLower());

                if (alreadyExists)
                {
                    ModelState.AddModelError("Book.Title", "You already have this book on your shelf!");
                    return View(userbook);
                }
            }

            if (userbook.Book == null)
            {
                userbook.Book = new Book();
            }
            userbook.Book.Title = formTitle;
            userbook.Book.Author = formAuthor;
            userbook.Book.Genre = formGenre;
            userbook.Book.AgeRating = formAgeRating;
            userbook.Book.Description = formDescription;
            userbook.Book.ReviewId = null;

            if (coverFile != null && coverFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "covers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(coverFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    coverFile.CopyTo(fileStream);
                }
                userbook.Book.ImageURL = "/covers/" + uniqueFileName;
            }

            if (!string.IsNullOrEmpty(userbook.Book.Title))
            {
                _context.Books.Add(userbook.Book);
                _context.SaveChanges();

                userbook.BookId = userbook.Book.BookId;

                _context.UserBooks.Add(userbook);
                _context.SaveChanges();

                decimal parsedRating = 0;
                if (!string.IsNullOrEmpty(reviewRating) &&
                    decimal.TryParse(reviewRating, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedRating))
                {
                    if (parsedRating > 0)
                    {
                        var newReview = new Review
                        {
                            Rating = parsedRating,
                            ReviewComment = reviewComment ?? "",
                            UserBook = userbook
                        };

                        _context.Reviews.Add(newReview);
                        _context.SaveChanges();

                        userbook.ReviewId = newReview.ReviewId;
                        userbook.Book.ReviewId = newReview.ReviewId;
                        _context.SaveChanges();
                    }
                }

                return RedirectToAction("Index", "Shelf");
            }
            ModelState.AddModelError("Book.Title", "Title or author cannot be empty!");
            return View(userbook);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.Review).Include(ub => ub.Borrows)
                .FirstOrDefault(ub =>
            ub.UserBookId == id &&
            (
                ub.ApplicationUserId == currentUserId
                ||
                ub.Borrows.Any(b => b.ApplicationUserId == currentUserId && b.StatusBorrow == EnumStatusBorrow.Borrowed)
            ));

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
                .Include(ub => ub.Book).Include(ub => ub.Borrows)
                .FirstOrDefault(ub => ub.UserBookId == id &&
                    (ub.ApplicationUserId == currentUserId ||
                     ub.Borrows.Any(b => b.ApplicationUserId == currentUserId && b.StatusBorrow == EnumStatusBorrow.Borrowed))
                );

            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            return View(userBook);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UserBook userbook)
        {
            if (id != userbook.UserBookId)
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);

            var dbUserBook = _context.UserBooks
        .Include(ub => ub.Borrows)
        .FirstOrDefault(ub => ub.UserBookId == id);

            if (dbUserBook == null)
            {
                return NotFound();
            }

            bool isOwner = dbUserBook.ApplicationUserId == currentUserId;
            bool isBorrower = dbUserBook.Borrows.Any(b => b.ApplicationUserId == currentUserId && b.StatusBorrow == EnumStatusBorrow.Borrowed);

            if (!isOwner && !isBorrower)
            {
                return Forbid();
            }

            if (isOwner)
            {
                // --- LOGIKA DLA WŁAŚCICIELA (Oryginalna) ---
                dbUserBook.Media = userbook.Media;
                dbUserBook.ReadStatus = userbook.ReadStatus;
                dbUserBook.Location = userbook.Location;
                dbUserBook.Notes = userbook.Notes;

                if (userbook.Media != EnumMedia.Printed)
                {
                    dbUserBook.Bookmark = false;
                    dbUserBook.IsOwned = false;
                    dbUserBook.IsBorrowed = false;
                    dbUserBook.Location = null;
                }
                else
                {
                    if (userbook.IsOwned) { dbUserBook.IsBorrowed = false; dbUserBook.IsOwned = true; }
                    else if (userbook.IsBorrowed) { dbUserBook.IsOwned = false; dbUserBook.IsBorrowed = true; }
                }
            }
            else if (isBorrower)
            {
                dbUserBook.ReadStatus = userbook.ReadStatus;
                dbUserBook.Location = userbook.Location;
                dbUserBook.Notes = userbook.Notes;

            }

            try
            {
                _context.UserBooks.Update(dbUserBook);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UserBooks.Any(e => e.UserBookId == userbook.UserBookId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index", "Shelf");
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