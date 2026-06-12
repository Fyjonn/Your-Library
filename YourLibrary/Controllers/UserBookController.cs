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

/// <summary>
/// Kontroler odpowiadajacy za zarzadzanie polka uzytkownika - dodawanie ksiazke, edycja (podzial na wlascicela, aktualnie wypozyczajecego, uzytkownika, ktory oddal ksiazke),
/// podglad informacji o pozycji (podzial na wlascicela, aktualnie wypozyczajecego, uzytkownika, ktory oddal ksiazke), usuwanie pozycji.
/// </summary>

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

        // tworzenie pozycji na polce - get
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserBook { Book = new Book() });
        }

        // tworzenie pozycji na polce - post
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
                        string finalComment = string.IsNullOrWhiteSpace(reviewComment)? "Rating only (no review comment provided).": reviewComment;

                        var newReview = new Review
                        {
                            Rating = parsedRating,
                            ReviewComment = finalComment,
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

        // wyswietlanie detali - get
        [HttpGet]
        public IActionResult Details(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.Review)
                .Include(ub => ub.ApplicationUser)
                .Include(ub => ub.Borrows)
                    .ThenInclude(b => b.ApplicationUser)
                .FirstOrDefault(ub => ub.UserBookId == id);

            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            bool isOwner = userBook.ApplicationUserId == currentUserId;

            var userBorrowRecord = userBook.Borrows?
                .Where(b => b.ApplicationUserId == currentUserId)
                .OrderByDescending(b => b.BorrowDate)
                .FirstOrDefault();

            bool isBorrower = userBorrowRecord != null;

            if (!isOwner && !isBorrower)
            {
                return Forbid();
            }

            if (!isOwner && userBorrowRecord != null)
            {
                userBook.Location = userBorrowRecord.BorrowerLocation;
                userBook.Notes = userBorrowRecord.BorrowerNotes;
                userBook.Bookmark = userBorrowRecord.BorrowerBookmark;
                userBook.ReadStatus = userBorrowRecord.BorrowerFinalReadStatus ?? EnumReadStatus.ToRead;

                userBook.Review = new Review
                {
                    Rating = userBorrowRecord.BorrowerRating,
                    ReviewComment = userBorrowRecord.BorrowerReviewComment ?? string.Empty
                };
            }

            return View(userBook);
        }

        // edycja detali - get
        [HttpGet]
        public IActionResult Edit(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userBook = _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.Review)
                .Include(ub => ub.Borrows)
                .FirstOrDefault(ub => ub.UserBookId == id);

            if (userBook == null || userBook.Book == null)
            {
                return NotFound();
            }

            bool isOwner = userBook.ApplicationUserId == currentUserId;

            var userBorrowRecord = userBook.Borrows?
                .Where(b => b.ApplicationUserId == currentUserId)
                .OrderByDescending(b => b.BorrowDate)
                .FirstOrDefault();

            if (!isOwner && userBorrowRecord == null)
            {
                return Forbid();
            }

            if (!isOwner && userBorrowRecord != null)
            {
                userBook.Location = userBorrowRecord.BorrowerLocation;
                userBook.Notes = userBorrowRecord.BorrowerNotes;
                userBook.Bookmark = userBorrowRecord.BorrowerBookmark;
                userBook.ReadStatus = userBorrowRecord.BorrowerFinalReadStatus ?? EnumReadStatus.ToRead;

                userBook.Review = new Review
                {
                    Rating = userBorrowRecord.BorrowerRating,
                    ReviewComment = userBorrowRecord.BorrowerReviewComment ?? string.Empty
                };
            }

            return View(userBook);
        }

        // edycja detali - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UserBook userbook, string? reviewComment)
        {
            if (id != userbook.UserBookId)
            {
                return NotFound();
            }

            string currentUserId = _userManager.GetUserId(User);

            var dbUserBook = _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.Review)
                .Include(ub => ub.Borrows)
                .FirstOrDefault(ub => ub.UserBookId == id);

            if (dbUserBook == null)
            {
                return NotFound();
            }

            bool isOwner = dbUserBook.ApplicationUserId == currentUserId;

            var userBorrowRecord = dbUserBook.Borrows?
                .Where(b => b.ApplicationUserId == currentUserId)
                .OrderByDescending(b => b.BorrowDate)
                .FirstOrDefault();

            bool isCurrentBorrower = userBorrowRecord != null && userBorrowRecord.StatusBorrow == EnumStatusBorrow.Borrowed;
            bool isPastBorrower = userBorrowRecord != null && (userBorrowRecord.StatusBorrow == EnumStatusBorrow.Returned || userBorrowRecord.StatusBorrow == EnumStatusBorrow.Completed);

            if (!isOwner && !isCurrentBorrower && !isPastBorrower)
            {
                return Forbid();
            }

            string rawRating = Request.Form["reviewRating"].ToString();
            decimal parsedRating = 0;

            if (!string.IsNullOrEmpty(rawRating))
            {
                if (!decimal.TryParse(rawRating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedRating))
                {
                    decimal.TryParse(rawRating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out parsedRating);
                }
            }

            if (isOwner)
            {
                if (userbook.Book == null || string.IsNullOrWhiteSpace(userbook.Book.Title) || string.IsNullOrWhiteSpace(userbook.Book.Author))
                {
                    ModelState.AddModelError("", "Title and Author are required to save a book!");
                    return View(dbUserBook);
                }

                if (dbUserBook.Book != null)
                {
                    dbUserBook.Book.Title = userbook.Book.Title;
                    dbUserBook.Book.Author = userbook.Book.Author;
                    dbUserBook.Book.Genre = userbook.Book.Genre;
                    dbUserBook.Book.AgeRating = userbook.Book.AgeRating;
                    dbUserBook.Book.Description = userbook.Book.Description;
                }

                dbUserBook.Media = userbook.Media;
                dbUserBook.ReadStatus = userbook.ReadStatus;
                dbUserBook.Location = userbook.Location;
                dbUserBook.Notes = userbook.Notes;
                dbUserBook.Bookmark = userbook.Bookmark;
                dbUserBook.IsOwned = userbook.IsOwned;

                if (userbook.Media != EnumMedia.Printed)
                {
                    dbUserBook.Bookmark = false;
                    dbUserBook.IsOwned = false;
                    dbUserBook.Location = null;
                }

                bool ownerCanHaveReview = userbook.ReadStatus == EnumReadStatus.Read || userbook.ReadStatus == EnumReadStatus.DNF;
                if (ownerCanHaveReview)
                {
                    if (dbUserBook.Review == null)
                    {
                        dbUserBook.Review = new Review { Rating = parsedRating, ReviewComment = reviewComment ?? "" };
                    }
                    else
                    {
                        dbUserBook.Review.Rating = parsedRating;
                        dbUserBook.Review.ReviewComment = reviewComment ?? "";
                    }
                }
                else if (!ownerCanHaveReview && dbUserBook.Review != null)
                {
                    dbUserBook.Review.Rating = 0;
                    dbUserBook.Review.ReviewComment = string.Empty;
                }

                _context.UserBooks.Update(dbUserBook);
            }
            else
            {
                if (userBorrowRecord != null)
                {
                    dbUserBook.ReadStatus = userbook.ReadStatus;

                    userBorrowRecord.BorrowerLocation = userbook.Location;
                    userBorrowRecord.BorrowerNotes = userbook.Notes;
                    userBorrowRecord.BorrowerBookmark = userbook.Bookmark;
                    userBorrowRecord.BorrowerFinalReadStatus = userbook.ReadStatus;

                    bool borrowerCanHaveReview = userbook.ReadStatus == EnumReadStatus.Read || userbook.ReadStatus == EnumReadStatus.DNF;

                    if (borrowerCanHaveReview)
                    {
                        userBorrowRecord.BorrowerRating = parsedRating;
                        userBorrowRecord.BorrowerReviewComment = reviewComment ?? string.Empty;
                    }
                    else
                    {
                        userBorrowRecord.BorrowerRating = 0;
                        userBorrowRecord.BorrowerReviewComment = string.Empty;
                    }

                    _context.Entry(dbUserBook).State = EntityState.Modified;
                    _context.Entry(userBorrowRecord).State = EntityState.Modified;
                }
            }

            try
            {
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

        // usuwanie pozycji - get
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

        // potwierdzenie usuniecia pozycji - post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string currentUserId = _userManager.GetUserId(User);

            var userbook = _context.UserBooks
                .FirstOrDefault(x => x.UserBookId == id && x.ApplicationUserId == currentUserId);
            if (userbook == null)
            {
                return NotFound();
            }

            if (userbook.IsBorrowed)
            {
                TempData["FriendError"] = "You cannot delete a book that is currently borrowed!";
                return RedirectToAction("Index", "Shelf");
            }

            _context.UserBooks.Remove(userbook);
            _context.SaveChanges();

            return RedirectToAction("Index", "Shelf");
        }
    }
}