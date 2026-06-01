using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Data;
using YourLibrary.Models;

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

        public IActionResult Index()
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