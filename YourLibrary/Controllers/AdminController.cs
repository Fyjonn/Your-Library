using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize(Roles = "Admin")] // Tylko dla osób z rolą Admin
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            return View(await users.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> GetUserReviews(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var reviews = await _context.Reviews
                .Include(r => r.UserBook)
                    .ThenInclude(ub => ub.Book)
                .Where(r => r.UserBook != null && r.UserBook.ApplicationUserId == id)
                .OrderByDescending(r => r.ReviewId)
                .Select(r => new
                {
                    bookTitle = r.UserBook.Book != null ? r.UserBook.Book.Title : "Unknown Book",
                    bookAuthor = r.UserBook.Book != null ? r.UserBook.Book.Author : "Unknown Author",
                    rating = r.Rating,
                    comment = r.ReviewComment ?? "No comment provided."
                })
                .ToListAsync();

            return Json(reviews);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id)
            {
                TempData["AdminError"] = "You cannot delete your own admin account!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userFriendships = await _context.Friendships
                    .Where(f => f.RequesterId == id || f.ReceiverId == id)
                    .ToListAsync();
                if (userFriendships.Any()) _context.Friendships.RemoveRange(userFriendships);

                var userReviews = await _context.Reviews
                    .Where(r => r.UserBook != null && r.UserBook.ApplicationUserId == id)
                    .ToListAsync();

                if (userReviews.Any())
                {
                    var reviewIds = userReviews.Select(r => r.ReviewId).ToList();

                    var linkedBooks = await _context.Books
                        .Where(b => b.ReviewId != null && reviewIds.Contains(b.ReviewId.Value))
                        .ToListAsync();

                    foreach (var book in linkedBooks)
                    {
                        book.ReviewId = null;
                    }

                    var linkedUserBooks = await _context.UserBooks
                        .Where(ub => ub.ReviewId != null && reviewIds.Contains(ub.ReviewId.Value))
                        .ToListAsync();

                    foreach (var ub in linkedUserBooks)
                    {
                        ub.ReviewId = null;
                    }

                    await _context.SaveChangesAsync();

                    _context.Reviews.RemoveRange(userReviews);
                    await _context.SaveChangesAsync();
                }

                var userBorrows = await _context.Borrows
                    .Include(b => b.UserBook)
                    .Where(b => b.ApplicationUserId == id)
                    .ToListAsync();

                foreach (var borrow in userBorrows)
                {
                    if (borrow.StatusBorrow == EnumStatusBorrow.Borrowed || borrow.StatusBorrow == EnumStatusBorrow.Requested)
                    {
                        borrow.StatusBorrow = EnumStatusBorrow.Completed;
                        borrow.ReturnDate = DateTime.Now;

                        if (borrow.UserBook != null)
                        {
                            borrow.UserBook.IsBorrowed = false;
                            borrow.UserBook.ReadStatus = borrow.OriginalOwnerReadStatus;
                        }
                    }
                }
                await _context.SaveChangesAsync();

                if (userBorrows.Any()) _context.Borrows.RemoveRange(userBorrows);

                var userBooks = await _context.UserBooks
                    .Where(ub => ub.ApplicationUserId == id)
                    .ToListAsync();
                if (userBooks.Any()) _context.UserBooks.RemoveRange(userBooks);

                await _context.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["AdminSuccess"] = $"User {user.UserName} and all their data have been deleted. Borrowed books safely returned to their owners!";
                }
                else
                {
                    TempData["AdminError"] = "Failed to delete user from Identity system.";
                }
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = $"An error occurred while deleting the user: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}