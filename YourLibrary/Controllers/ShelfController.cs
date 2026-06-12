using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

/// <summary>
/// Kontroler odpowiadajcy za wyswietlanie polki uzytkownika.
/// </summary>

namespace YourLibrary.Controllers
{
    [Authorize]
    public class ShelfController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShelfController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            string currentUserId = _userManager.GetUserId(User);

            var myShelfBooks = await _context.UserBooks
                .Include(ub => ub.Book)
                .Include(ub => ub.Borrows)
                .Where(ub => ub.ApplicationUserId == currentUserId ||
                             ub.Borrows.Any(b => b.ApplicationUserId == currentUserId &&
                               (b.StatusBorrow == EnumStatusBorrow.Borrowed ||
                                b.StatusBorrow == EnumStatusBorrow.Returned ||
                                b.StatusBorrow == EnumStatusBorrow.Completed)))
                .ToListAsync();
            return View(myShelfBooks);
        }
    }
}